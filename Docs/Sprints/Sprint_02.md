# Sprint 02 — Level01 jugable, enemigo bioluminiscente y audio

> **Estado:** 🔄 En progreso · **Última revisión:** 2026-06-23 · **Ref. GDD:** §8, §9.2, §13, §14

Geometría y parallax de Level01, animaciones completas del rover, primer enemigo con IA (Ser Bioluminiscente) y mejoras de audio.

## Equipo y ramas

| Rol | Rama | Tareas en este sprint |
|-----|------|-----------------------|
| Gameplay Programmer (Líder) | `feature/player-movement` | T2 (integración animator), deuda S01 |
| Artist | `feature/vfx-shaders` / arte | T1 (fondos, tileset), T2 (sprites) |
| Level Designer | `feature/level-design` | T5 (tilemap), T1 (montaje parallax) |
| AI Programmer | `feature/enemy-ai` | T3 (cierre Biol) |
| Technical Director | `feature/audio-system` | T4 (Audio Mixer) |

## Ya completado (no modificar salvo bug)

- **Ser Bioluminiscente:** FSM Idle/Alert/Chase (`BioluminescentAI.cs`), ataque integrado (`BiolAttack.anim` + trigger `Attack`), `ApplyStun()`, log de daño al contacto (`OnTriggerStay2D`). Prefab `SerBioluminiscente.prefab`, `BiolAC.controller`, escena `Enemy/BiolAC.unity`.
- **EnemyStatsSO** completo: `hp`, `contactDamage`, `moveSpeedBase`, `moveSpeed`, `alertRange`, `loseRange`, `loseTime`, `alertTime`, `stunDuration`.
- **Animaciones rover en RoverAC:** Idle, Walk, Run, Jump, Scan, **Dash**, **Death** (params `Speed`, `IsGrounded`, `IsJumping`, `IsScanning`, `VelocityY`, `IsDashing`, `IsOnWall`, `IsDead`, `IsDamaged`). Dash verificado en Play Mode; `dashDuration` 0.5 s para encajar el clip de 6 frames. Death montado y terminal (a la espera de `OnDeath` en Sprint 03).
- **Tooling de nivel:** `PlaceholderTileGenerator.cs` (menú `Tools → Red Dust`), `LevelMarker.cs` (7 tipos), 5 tiles placeholder + palette.
- **Arte disponible:** sprites de dash del rover, fondos PNG de cuevas en `Art/Backgrounds/`.
- **Mecánicas de nivel nuevas (`Scripts/leveo01/`, no planificadas en el sprint original):** `PlataformaMovil` (plataforma horizontal/vertical con ida y vuelta), `GiroCompleto` (rotación continua), `OsciladorGiro` (oscilación tipo péndulo por seno), `CaidaCristal` y `CaidaPorCercania` (trampas de cristal que caen al detectar al Player por raycast). Las tres primeras ya están **en uso en la escena `Level01_2.0/Dev_PlayerMovement.unity`**; las dos trampas existen pero aún no se colocan en escena.

---

## 🟡 T5 — Geometría de Level01
**Responsable:** Level Designer · **Rama:** `feature/level-design` · **Ref. GDD:** §9.2, §16.1

**Avance (2026-06-23):**
- ✅ **Tileset real** (`piso_1_mundo`) sliceado a 64 px (PPU 64) y >100 tiles generados en `Assets/Art/Tiles/` con paleta lista (ya no son placeholders).
- ✅ **Geometría jugable construida** en la escena **`Assets/Scenes/Level01_2.0/Dev_PlayerMovement.unity`** (~966 KB): Grid + **6 `Tilemap`** y **2 `CompositeCollider2D`** montados; el Player se apoya, corre, salta y hace dash sobre suelo real.
- ✅ **Prefabs instanciados en escena:** 1 `Player.prefab` y 1 `SerBioluminiscente.prefab` (con `rover` asignado). Mecánicas activas: **4 `PlataformaMovil`**, **3 `GiroCompleto`**, **3 `OsciladorGiro`**.
- 🔴 **Desincronización de escenas:** la `Level01.unity` "oficial" **sigue vacía** (solo la cámara Cinemachine: 0 tilemaps, 0 prefabs). El trabajo real vive en la escena paralela `Level01_2.0/Dev_PlayerMovement.unity`. **Decisión pendiente:** promover esa escena a `Level01.unity` (y actualizar `SceneLoader` + Build Settings) o copiar el contenido. Mientras no se haga, `SceneLoader.LoadLevel01()` carga una escena vacía.
- 🔴 **Pendiente:** **colocar marcadores `LevelMarker`** (ninguna escena los tiene todavía); colocar las trampas `CaidaCristal`/`CaidaPorCercania`; cerrar el beat map de las 5 zonas (Z1→Z5).

> Guía paso a paso: [T5_Geometria_Level01.md](./T5_Geometria_Level01.md).

### Montaje del Grid
1. Crear GameObject raíz `Grid` (componente `Grid`, cell size 1×1).
2. Hijo `Collision_Tilemap`: layer **`Ground`**, componentes `Tilemap` + `TilemapRenderer` + `TilemapCollider2D` + `CompositeCollider2D`; añadir `Rigidbody2D` (Body Type **Static**) y marcar `TilemapCollider2D → Used By Composite`. Pintar suelo/paredes/techo con `Tile_Ground_Placeholder`.
3. Hijo `OneWay_Tilemap` (opcional): plataformas con `Tile_Platform_Placeholder`; usar `PlatformEffector2D` para colisión unidireccional.
4. Hijo `Visual_Tilemap`: layer `Default`, sin colliders (decoración).

### Beat map (usar `LevelMarker` para marcar, no para colisión)
| Zona | Contenido | Marcadores |
|------|-----------|-----------|
| Z1 Entrada/tutorial | suelo plano, saltos simples | `SpawnPoint`, `Scannable` (SC-01) |
| Z2 Primera tensión | foso que obliga a Dash, sala lateral inaccesible | `EnemyPatrol`, `BlockedZone`, `Checkpoint` |
| Z3 Respiro | sala segura sin enemigos | `Scannable` (SC-02), `EnergyCellPickup` |
| Z4 Escalada vertical | tramo angosto para Wall Jump, plataformas a alturas | `Scannable` (SC-03), `Checkpoint` |
| Z5 Boss | sala amplia horizontal despejada | `BossRoom` |

### Integración de prefabs
- Instanciar `Player.prefab` en el `SpawnPoint` de Z1.
- Instanciar `SerBioluminiscente.prefab` en las zonas `EnemyPatrol`; asignar el campo `rover` del `BioluminescentAI` al Transform del Player en escena.
- Marcar ~30% del recorrido con `Tile_Blocked_Placeholder` + `BlockedZone` (zonas que exigirán el upgrade Rueda Reforzada).

**Dependencia:** los layers `Player`/`Enemy`/`Platform`/`Interactable` deben existir antes (ver Config de Unity).

**DoD:** se recorre Z1→Z4 con el prefab del jugador sin caer al vacío ni atascarse; la colisión con `Ground` es estable; las zonas bloqueadas están delimitadas visualmente.

---

## 🟡 T1 — Fondos y parallax de Level01
**Responsable:** Artist (arte) + Level Designer (montaje) · **Rama:** `feature/level-design` · **Ref. GDD:** §9.1, §13.1

Los fondos PNG ya existen en `Art/Backgrounds/`; el script de desplazamiento **ya existe**; falta el montaje en escena. El tileset ya es real (ver T5).

### ✅ Script — `Scripts/Level/ParallaxBackground.cs` (clase `ParallaxController`)
- Ya implementado y en el repo (junto con `Parallax.cs`, `ParallaxCamera.cs`, `ParallaxLayer.cs`). Corre en `LateUpdate()`.
- **Nota de implementación:** la versión actual usa **desplazamiento de `_MainTex` por material** (`mat[i].SetTextureOffset`) calculando la velocidad por capa a partir de la `z` del hijo, en lugar del enfoque "mover el Transform por `delta * parallaxFactor`" descrito originalmente. Funciona para tiling; requiere materiales con textura wrap. Revisar si se mantiene este enfoque o se unifica con `ParallaxLayer`.
- 🔴 **Pendiente:** no está instanciado en ninguna escena (0 referencias en Level01/Dev). Falta el montaje de las 3 capas y asignar fondos/factores.

### Montaje (3 capas)
| Capa | Factor | Contenido sugerido |
|------|--------|--------------------|
| Lejana | 0.15× | cristal/niebla (`cristal_azul`, `exporex`) |
| Media | 0.45× | formaciones (`cristales_azules_recta`, `hongos_luminosos`) |
| Foreground | 0.85× | rocas / bordes en primer plano |

Cada capa es un hijo con `SpriteRenderer`; ajustar `Sorting Layer`/`Order in Layer` para que queden detrás del tilemap de juego.

### Tileset final (Artist)
Exportar spritesheet de cuevas (suelo, pared, techo, bordes, esquinas interior/exterior, plataforma one-way) a `Art/Sprites/Tilesets/` y reemplazar placeholders cuando esté listo.

**DoD:** las 3 capas se desplazan a distinta velocidad relativa a la cámara al mover al jugador, sin bordes visibles ni saltos.

---

## 🟢 T2 — Animaciones restantes del rover
**Responsable:** Artist (sprites) + Gameplay Programmer (integración) · **Rama:** `feature/player-movement` · **Ref. GDD:** §13.3 · **Arq.:** [AnimatorSetup.md](../Architecture/AnimatorSetup.md)


### Pendiente — `Land` y `Damage` (bloqueado por Artist)
1. Artist: crear `Opportunity-land` (4f, no-loop) y `Opportunity-damage` (5f, no-loop) en `Art/Sprites/Oppy/`.
2. Crear los clips `Rover_Land` y `Rover_Damage` y añadir sus estados al `RoverAC` con transiciones instantáneas (Duration 0, Has Exit Time OFF, Write Defaults OFF):
   - `Land` ← al pasar a grounded (transición desde Jump/caída con `IsGrounded == true`).
   - `Damage` ← trigger `IsDamaged`.
3. El trigger `IsDamaged` ya está en el controller pero se *escribe* desde código cuando **Sprint 03 (T1)** añada el evento `OnDamageReceived` (hoy comentado en `PlayerAnimatorController`).

### Diferido a V2 — `Wall_Slide` / `Wall_Jump`
Requieren `Opportunity-wallslide` (6f, loop) y `Opportunity-walljump` (5f). El parámetro `IsOnWall` ya queda cableado (`OnWallSliding`) para cuando se aborden.

**DoD (restante):** cuando existan los sprites, `Land` y `Damage` se reproducen en Play Mode con transiciones instantáneas y Write Defaults OFF.

---

## 🟡 T3 — Cierre del Ser Bioluminiscente
**Responsable:** AI Programmer · **Rama:** `feature/enemy-ai` · **Ref. GDD:** §8.2

El FSM, el ataque y `ApplyStun()` ya funcionan. Falta la muerte/HP y conectar el aturdimiento al escaneo. **Estado al 2026-06-23:** `OnTriggerStay2D` sigue solo logueando (no llama a `TakeDamage`), no existe campo `_currentHp` ni `TakeDamage`/`Die`, y nada invoca `ApplyStun()` desde el escaneo.

**Adelanto de assets (2026-06-23):** el sprite de muerte `Bio_death.png` ya está en `Art/Sprites/Enemy/` (aún sin trackear en git) y `BiolAC.controller` ya tiene el parámetro `IsDead` + un estado vacío reservado. Falta crear `BiolDeath.anim`, cablear el estado `Death` con el trigger/bool de muerte y escribir la lógica `TakeDamage`/`Die` que lo dispare.

### Lógica de HP y muerte (en `BioluminescentAI.cs`)
1. Campo runtime `private int _currentHp;` inicializado en `Start()` con `stats.hp`.
2. Método público `public void TakeDamage(int amount)` → resta a `_currentHp`; si `<= 0`, llamar `Die()`.
3. `Die()`: poner trigger `Death` en el animator, deshabilitar collider y el FSM (`enabled = false`), y `Destroy(gameObject, delay)` tras la animación.
4. Estado `Death` en `BiolAC.controller`: requiere sprite + `BiolDeath.anim` (Artist crea el sprite).

### Aturdimiento por pulso de escaneo
- El rover ya expone `IsScanning` (propiedad pública en `PlayerController`).
- Opción recomendada: que el Biol, mientras está en rango, verifique `rover.GetComponent<PlayerController>().IsScanning` y llame su propio `ApplyStun()` (cooldown interno para no reaplicar cada frame). Mantiene la lógica del lado del enemigo y evita acoplar el Player a los enemigos.

**DoD:** el Biol persigue al rover; al recibir daño suficiente reproduce Death y se desactiva; al escanearlo en rango queda aturdido `stunDuration` segundos.

---

## 🟡 T4 — Audio Mixer y estados de música
**Responsable:** Technical Director · **Rama:** `feature/audio-system` · **Ref. GDD:** §14

`AudioManager` sigue básico (`PlayMusic`, `TriggerGameOverMusic`, `PlayGlobalSFX`). **Sin avances al 2026-06-22:** no existe ningún `.mixer` en el proyecto, ni el `enum MusicState`, ni `SetMusicState`. (Nota: `PlayerAudioController` ya tiene `PlayDeathSound`/`sfxDeath`/`sfxDamage` listos.)

### Audio Mixer (4 buses)
Crear `Assets/Audio/MainMixer.mixer` con grupos **Master → {Music, SFX, Ambient}**. Enrutar:
- `AudioManager.musicSource` → grupo **Music**.
- `PlayerAudioController.loopAudioSource` y `oneShotAudioSource` → grupo **SFX**.
- Exponer parámetros de volumen (`MusicVol`, `SfxVol`, `AmbientVol`) para futuros ajustes de opciones.

### Estados de música (en `AudioManager.cs`)
- `public enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }`.
- `public void SetMusicState(MusicState state)` con crossfade entre clips (corutina que baja el `musicSource` actual y sube el nuevo). Tiempos: Exploration→Tension 1.5 s, Tension→Combat 0.5 s, Combat→Exploration 3.0 s.

### SFX de enemigos
Agregar al menos SFX de ataque y de muerte por enemigo en `Audio/SFX/` y dispararlos desde el script de IA correspondiente vía el bus SFX.

**DoD:** la música transiciona con crossfade entre estados; el Mixer permite ajustar Music/SFX/Ambient por separado; los AudioSources del rover suenan por el bus SFX.

---

## 🟢 Deuda técnica de Sprint 01
**Responsable:** Gameplay Programmer · **Rama:** `feature/player-movement`

> ⚠️ **Ambos puntos siguen pendientes al 2026-06-22.**

- **Bug `OnDashed`:** en `PlayerAudioController.cs:39/50` la suscripción **sigue siendo** `OnDashed += PlayDamageSound`. Añadir `[SerializeField] AudioClip sfxDash;` + método `PlayDashSound()` (one-shot) y suscribir `OnDashed += PlayDashSound`; o removerla si no habrá SFX de dash.
- **Bindings vs GDD §2:** en `RoverInputActions_Local.inputactions`, Jump **sigue** en `W`/`↑` (GDD pide `Espacio`) y Scan en `E` (GDD pide `F`). Ajustar los bindings.

---

## Configuración de Unity pendiente
**Responsable:** Technical Director (define) + cada dev al usar su layer

- Crear layers: `Player`, `Enemy`, `Platform`, `Interactable` (`Ground` ya existe).
- Ajustar la **Layer Collision Matrix** (`Project Settings → Physics 2D`) según GDD §16.1.
- Mantener: Simulation Mode `Fixed Update`, material `NoFriction` en Player/Ground, Collision Detection `Continuous` en RB2D dinámicos.

## Reglas del proyecto

- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin avisar al Gameplay Programmer; no tocar Physics 2D ni el Canvas de referencia (1920×1080) sin el Technical Director.
- Conventional Commits; rebase sobre `develop` antes del PR; nunca push directo a `main`/`develop`; **siempre** incluir `.meta`; nunca subir `Library/`.

## Progreso

> Revisión 2026-06-23.

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T5 — Geometría Level01 | Level Designer | 🔴 Alta | ~70% (tileset real + geometría jugable con prefabs y mecánicas ✅ en `Level01_2.0`; **falta consolidar en `Level01.unity`** + marcadores + trampas + cierre de zonas) |
| T1 — Fondos/parallax | Artist + Level Designer | 🟡 Media | ~50% (fondos ✅, **script `ParallaxController` ✅**; falta montaje de capas en escena) |
| T2 — Animaciones rover | Artist + Gameplay | 🟢 Baja | ~85% (Dash/Death ✅; falta Land/Damage por sprites + evento S03; Wall_* es V2) |
| T3 — Cierre Biol | AI Programmer | 🟡 Media | ~80% (FSM/ataque/stun ✅, sprite `Bio_death` ✅; falta `BiolDeath.anim` + muerte/HP + stun-por-escaneo) |
| T4 — Audio Mixer | Technical Director | 🟡 Media | 0% (sin `.mixer` ni `MusicState`) |
| Deuda Sprint 01 | Gameplay | 🟢 Baja | pendiente (audio bug `OnDashed→PlayDamageSound` + bindings Jump/Scan) |
| ➕ Mecánicas de nivel | Level Designer | — | extra fuera de plan: `PlataformaMovil`/`GiroCompleto`/`OsciladorGiro` ✅ en uso; `CaidaCristal`/`CaidaPorCercania` por colocar |
