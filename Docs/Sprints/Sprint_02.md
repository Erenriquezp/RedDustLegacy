# Sprint 02 — Nivel 1 jugable, enemigo bioluminiscente y audio

> **Objetivo:** Level01 con geometría base y parallax, animaciones del rover completas, primer enemigo con IA (Ser Bioluminiscente) y sistema de audio Mixer/música.  
> **Estado:** 🔄 En progreso  
> **Última actualización:** 2026-06-18  
> **Ref. GDD:** §8 IA, §9.2 Nivel 1, §13 Animación, §14 Audio  
> **Prerequisito:** [Sprint 01 MVP](RedDustLegacy/Docs/Sprints/Sprint_01_MVP.md) completado

---

## Inventario del proyecto — Estado actual

### Assets existentes (verificados en el repositorio)

**Sprites del Rover** (`Assets/Art/Sprites/Oppy/`):

| Archivo | Estado |
|---------|--------|
| `Opportunity-idle.png` | ✅ Integrado en RoverAC |
| `Opportunity-walk.png` | ✅ Integrado en RoverAC |
| `Opportunity-jump-v1.png` | ✅ Integrado en RoverAC |
| `Opportunity-run.png`  | ✅ Integrado en RoverAC |
| `Opportunity-scan_loop.png` | ✅ Integrado en RoverAC |
| `Opportunity-dash-v1.png` | ✅ Sprite creado — pendiente integración en RoverAC |
| `Opportunity-dash-v3.png` | ✅ Sprite creado (versión actualizada) — pendiente integración |
| `Opportunity-death.png` | ⬜ Existe, no integrado en RoverAC |
| `Opportunity-destruction.png` | ⬜ Existe, no integrado |
| `Opportunity-victory.png` | ⬜ Existe, no integrado |

> **Nota:** Los sprites del rover se reorganizaron de `Assets/Art/Sprites/` a `Assets/Art/Sprites/Oppy/`. La documentación del Sprint 01 usaba la ruta anterior.

**Sprites de Enemigos** (`Assets/Art/Sprites/` y `Assets/Art/Sprites/Enemy/`):

| Enemigo | Sprites existentes |
|---------|-------------------|
| Ser Bioluminiscente | `Enemy/Biol-idle-v1.png`, `Enemy/Biol-walk-v1.png`, `Enemy/Biol-bio_attack-v1.png` ✨ **NUEVO** |
| Drone Patrullero | `Drone-idle-v1.png`, `Drone-walk-v1.png` |
| Drone Detector | `Drone Detector-idle-v1.png` |
| Centinela Secundario | `Centinela Secundario-idle.png`, `CentinelaSecundaria-morir.png` |
| Centinela Principal | `CentinelaPrincipal-idle.png`, `CentinelaPrincipal-walk.png`, `CentinelaPrincipal-eletricAttack.png`, `CentinelaPrincipal-morir.png` |
| Leviatán | `Leviatan-idle.png`, `Leviatan-walk.png`, `Leviatan-ataque.png`, `Leviatan-muerte.png` |

> **Nota:** Los sprites del Biol se movieron a `Assets/Art/Sprites/Enemy/`. Los demás enemigos permanecen en `Assets/Art/Sprites/`. Verificar si se consolida toda la carpeta Enemy. Los sprites de concept art (`CentinelaPrincipal.png`, `CentinelaSecundaria.png`, `Leviatan.png`) ya **no se encuentran** en el repositorio.

**Audio** (`Assets/Audio/`):

| Tipo | Archivos | Ruta |
|------|----------|------|
| BGM | `Bgm_MainTheme.wav`, `Bgm_GameOver.wav` | `Audio/Music/` |
| SFX Rover | `Sfx_Rover_Idle.wav`, `Sfx_Rover_Walk.wav`, `Sfx_Rover_Jump.wav`, `Sfx_Rover_Damage.wav`, `Sfx_Rover_Death.wav` | `Audio/SFX/` |

> **Nota:** Audio se reestructuró de `Assets/Audio/` plano a subdirectorios `Music/` y `SFX/`.

**Animaciones Rover** (`Assets/Animations/Rover/`):

| Archivo | Integrado en RoverAC |
|---------|:---:|
| `Rover_Idle.anim` | ✅ |
| `Rover_walk.anim` | ✅ |
| `Rover_Jump.anim` | ✅ |
| `Rover_Run.anim` | ✅ |
| `Rover_Scan.anim` | ✅ |
| `RoverAC.controller` | ✅ |

**Animaciones Enemigos** (`Assets/Animations/Enemies/Ser Bioluminiscente/`):

| Archivo | Estado |
|---------|--------|
| `BiolAC.controller` | ✅ Creado e integrado en prefab |
| `BiolIdle.anim` | ✅ Creado |
| `BiolWalk.anim` | ✅ Creado |

**Tiles** (`Assets/Art/Tiles/Placeholders/`):

| Asset | Estado |
|-------|--------|
| `Tile_Ground_Placeholder.asset` | ✅ Generado |
| `Tile_Platform_Placeholder.asset` | ✅ Generado |
| `Tile_Blocked_Placeholder.asset` | ✅ Generado |
| `Tile_Background_Placeholder.asset` | ✅ Generado |
| `Tile_Danger_Placeholder.asset` | ✅ Generado |
| `Placeholders_Palette.prefab` | ✅ Palette creada |

**Prefabs** (`Assets/Prefabs/`):

| Prefab | Ruta | Estado |
|--------|------|--------|
| `Player.prefab` | `Prefabs/Player/` | ✅ Creado e integrado |
| `SerBioluminiscente.prefab` | `Prefabs/Enemies/` | ✅ **NUEVO** — Creado e integrado |

**Escenas** (`Assets/Scenes/`):

| Escena | Ruta | Estado |
|--------|------|--------|
| `MainMenu.unity` | `Scenes/MainMenu/` | ✅ Sprint 01 |
| `Dev_PlayerMovement.unity` | `Scenes/Dev/` | ✅ Sprint 01 |
| `Level01.unity` | `Scenes/Level01/` | ✅ **NUEVO** — Escena creada |
| `Level02.unity` | `Scenes/Level02/` | ✅ **NUEVO** — Escena creada (placeholder futuro) |
| `BiolAC.unity` | `Scenes/Enemy/` | ✅ **NUEVO** — Escena de prueba del Biol |

**Scripts existentes:**

| Script | Ruta | Estado |
|--------|------|--------|
| `PlayerController.cs` | `Scripts/Player/` | ✅ Funcional — con eventos `OnGroundedChanged`, `OnJumped`, `OnDashed`, `OnWallJumped`, `OnWallSliding` |
| `PlayerAnimatorController.cs` | `Scripts/Player/` | ✅ Funcional — usa hashes de parámetros (`Speed`, `IsGrounded`, `IsJumping`, `IsScanning`) |
| `PlayerAudioController.cs` | `Scripts/Player/` | ⚠️ Bug: `OnDashed → PlayDamageSound` (línea 39) — debería ser `PlayDashSound()` |
| `RoverStatsSO.cs` | `Scripts/ScriptableObjects/` | ✅ Funcional |
| `RoverStats_Default.asset` | `Scripts/ScriptableObjects/` | ✅ Instancia creada |
| `SceneLoader.cs` | `Scripts/Core/` | ✅ Funcional |
| `AudioManager.cs` | `Scripts/Core/` | ⚠️ Básico, sin estados de música ni Audio Mixer |
| `MainMenuController.cs` | `Scripts/UI/` | ✅ Funcional |
| `BioluminescentAI.cs` | `Scripts/AI/` | ✅ **NUEVO** — FSM 3 estados (Idle/Alert/Chase) |
| `EnemyStatsSO.cs` | `Scripts/AI/ScriptableObjects/` | ✅ **NUEVO** — ScriptableObject para stats de enemigos |
| `BiolStats.asset` | `Scripts/AI/ScriptableObjects/` | ✅ **NUEVO** — Instancia con valores del GDD |
| `LevelMarker.cs` | `Scripts/Level/` | ✅ **NUEVO** — Marcadores visuales para Scene View (SpawnPoint, Checkpoint, Scannable, BlockedZone, EnemyPatrol, BossRoom, EnergyCellPickup) |
| `PlaceholderTileGenerator.cs` | `Scripts/Editor/` | ✅ **NUEVO** — Generador de tiles placeholder (menú Tools → Red Dust) |

**Documentación de arquitectura** (`Docs/Architecture/`):

| Documento | Estado |
|-----------|--------|
| `AnimatorSetup.md` | ✅ Existente |
| `PlayerController.md` | ✅ Existente |

**Lo que NO existe todavía:**
- ❌ Fondos/parallax (`Assets/Art/Backgrounds/` sigue vacío)
- ❌ Script `ParallaxController.cs` en `Assets/Scripts/Level/`
- ❌ Audio Mixer (sin configurar buses Master/Music/SFX/Ambient)
- ❌ Sprites faltantes del rover: `Opportunity-wallslide.png`, `Opportunity-walljump.png`, `Opportunity-land.png`, `Opportunity-damage.png`
- ❌ Animaciones faltantes del rover en RoverAC: `Rover_Dash.anim`, `Rover_Death.anim`, `Rover_Land.anim`, `Rover_Damage.anim`
- ❌ Animación de muerte del Biol: `BiolDeath.anim`
- ❌ Parámetros nuevos en RoverAC: `VelocityY`, `IsDashing`, `IsDamaged`, `IsDead`, `IsOnWall`
- ❌ SFX de enemigos
- ❌ Carpeta `Assets/Scripts/Audio/` está vacía

---

## Correcciones pendientes del Sprint 01

> Estas correcciones deben completarse **antes** de iniciar las tareas pendientes del Sprint 02.

| # | Corrección | Archivo | Detalle | Estado |
|---|-----------|---------|---------|--------|
| C-02 | Corregir suscripción OnDashed | `PlayerAudioController.cs` | Línea 39: `OnDashed += PlayDamageSound` debería ser un SFX de dash (`PlayDashSound()`) o removerse | ⚠️ Pendiente |

---

## Tarea 1 — Fondos y Parallax del Nivel 1

**Rama git:** `feature/level-design`  
**Responsable:** Level Designer + Artist  
**Ref. GDD:** §9.1 Técnicas 2.5D, §9.2 Nivel 1, §13.1 Paleta de colores  
**Estado global:** ❌ No iniciada

### 1.1 Fondos parallax (Artist)

**Estado:** ❌ No existen — `Assets/Art/Backgrounds/` está vacío

**Entregables:** 3 imágenes PNG de fondo para Nivel 1 (Cuevas Bioluminiscentes)

| Capa | Factor parallax | Descripción visual |
|------|----------------|-------------------|
| Fondo lejano | 0.15× | Pared de cristal lejana, niebla azulada |
| Formaciones medias | 0.45× | Cristales, columnas de roca, formaciones medianas |
| Foreground | 0.85× | Rocas en primer plano, bordes de cueva |

**Paleta de color Nivel 1 (GDD §13.1):**
- Negro cueva: `#0A0A12`
- Azul cristal: `#1A3A6E`
- Verde bioluminiscente: `#2ECC71`
- Turquesa detalle: `#00BCD4`
- Blanco destellos: `#E8F5E9`

**Guardar en:** `Assets/Art/Backgrounds/Level01/`

### 1.2 Tileset (Artist)

**Estado:** ⬜ Parcial — se usan tiles placeholder generados por `PlaceholderTileGenerator.cs` (5 tipos: Ground, Platform, Blocked, Background, Danger). Faltan los sprites finales del artista.

**Entregable:** Tileset de cuevas exportado como spritesheet

Piezas mínimas: suelo, pared, techo, borde, esquinas (interior y exterior), plataformas one-way.

**Guardar en:** `Assets/Art/Sprites/Tilesets/`

### 1.3 Parallax Controller (Technical Director)

**Estado:** ❌ No existe — no se encontró `ParallaxController.cs` en `Assets/Scripts/Level/`

**Entregable:** Script `ParallaxController.cs` en `Assets/Scripts/Level/`

```
Implementación:
- Crear GameObject ParallaxBackground en Level01
- Cada capa es un hijo con SpriteRenderer
- Desplazamiento = Camera.main.transform × factor
- Tiling horizontal para evitar bordes visibles
```

**Definition of Done:** Las 3 capas de parallax están importadas en la escena `Level01` y se desplazan a diferentes velocidades relativas a la cámara principal al moverse el jugador.

---

## Tarea 2 — Animaciones completas del Rover

**Rama git:** `feature/player-movement`  
**Responsable:** Artist + Gameplay Programmer  
**Ref. GDD:** §13.3 Animator Controller del Rover  
**Ref. Arquitectura:** [AnimatorSetup.md](file:///c:/Users/LENOVO/RedDustLegacy/Docs/Architecture/AnimatorSetup.md)  
**Estado global:** 🔄 Parcial — sprites de dash creados, falta integración

### 2.1 Sprites por crear (Artist)

| Sprite | Frames | FPS | Loop | Estado |
|--------|--------|-----|:----:|--------|
| `Opportunity-dash.png` | 6 | 24 | ☐ | ✅ Creado (`dash-v1.png`, `dash-v3.png`) |
| `Opportunity-wallslide.png` | 6 | 8 | ☑ | ❌ No existe |
| `Opportunity-walljump.png` | 5 | 12 | ☐ | ❌ No existe |
| `Opportunity-land.png` | 4 | 12 | ☐ | ❌ No existe |
| `Opportunity-damage.png` | 5 | 12 | ☐ | ❌ No existe |

### 2.2 Integración en RoverAC (Gameplay Programmer)

**Sprites existentes por integrar:**

| Sprite existente | Clip a crear/usar | Estado |
|-----------------|-------------------|--------|
| `Opportunity-run.png` | `Rover_Run.anim` (ya existe) | ✅ Integrado |
| `Opportunity-scan_loop.png` | `Rover_Scan.anim` (ya existe) | ✅ Integrado |
| `Opportunity-dash-v3.png` | Crear `Rover_Dash.anim` | ❌ No integrado |
| `Opportunity-death.png` | Crear `Rover_Death.anim` | ❌ No integrado |

**Nuevos estados en el Animator Controller:**

| Estado | Condición de entrada | Prioridad | Estado |
|--------|---------------------|-----------|--------|
| `Run` | `Speed > 0.1 && IsGrounded` | MVP | ✅ Integrado |
| `Jump` | `IsJumping = true` | MVP | ✅ Integrado (unificado, no separado Rise/Fall) |
| `Scan_Loop` | `IsScanning = true` | MVP | ✅ Integrado |
| `Dash` | `IsDashing = true` | MVP | ❌ Pendiente |
| `Death` | `IsDead = true` | MVP | ❌ Pendiente |
| `Land` | `IsGrounded (desde Jump)` | MVP | ❌ Pendiente (requiere sprite) |
| `Damage` | Trigger `IsDamaged` | MVP | ❌ Pendiente (requiere sprite) |
| `Wall_Slide` | `IsOnWall && VelocityY < 0` | V2 | ❌ Pendiente (requiere sprite) |
| `Wall_Jump` | Trigger `WallJumped` | V2 | ❌ Pendiente (requiere sprite) |

**Parámetros actualmente en PlayerAnimatorController.cs:**

| Parámetro | Tipo | Estado |
|-----------|------|--------|
| `Speed` | Float | ✅ Implementado — hash `_speedHash` |
| `IsGrounded` | Bool | ✅ Implementado — hash `_isGroundedHash` |
| `IsJumping` | Bool | ✅ Implementado — hash `_isJumpingHash` |
| `IsScanning` | Bool | ✅ Implementado — hash `_isScanningHash` |

**Parámetros nuevos a agregar en RoverAC y PlayerAnimatorController:**

| Parámetro | Tipo | Escrito por | Estado |
|-----------|------|-------------|--------|
| `VelocityY` | Float | `PlayerAnimatorController.Update()` | ❌ Pendiente |
| `IsDashing` | Bool | `PlayerAnimatorController` vía evento `OnDashed` | ❌ Pendiente |
| `IsDamaged` | Trigger | `PlayerAnimatorController` vía evento futuro `OnDamageReceived` | ❌ Pendiente |
| `IsDead` | Bool | `PlayerAnimatorController` vía evento futuro `OnDeath` | ❌ Pendiente |
| `IsOnWall` | Bool | `PlayerAnimatorController` vía evento `OnWallSliding` | ❌ Pendiente |

**Definition of Done:** El rover reproduce correctamente Idle, Run, Jump_Rise, Jump_Fall, Land, Dash, Scan y Death al probar en Play Mode. Las transiciones son instantáneas (Duration = 0, Has Exit Time = OFF). Write Defaults = OFF en todos los estados.

---

## Tarea 3 — Primer enemigo con IA: Ser Bioluminiscente

**Rama git:** `feature/enemy-ai`  
**Responsable:** AI Programmer  
**Ref. GDD:** §8.2 Ser Bioluminiscente  
**Estado global:** ✅ Base completada — FSM funcional, prefab creado, animaciones integradas

### 3.1 Scripts creados

| Script | Ruta | Estado | Notas |
|--------|------|--------|-------|
| `BioluminescentAI.cs` | `Scripts/AI/` | ✅ Creado | FSM de 3 estados (Idle/Alert/Chase), usa `EnemyStatsSO` |
| `EnemyStatsSO.cs` | `Scripts/AI/ScriptableObjects/` | ✅ Creado | Campos: `moveSpeed`, `alertRange`, `loseRange`, `loseTime`, `alertTime` |
| `BiolStats.asset` | `Scripts/AI/ScriptableObjects/` | ✅ Creado | Instancia con valores por defecto |

> **Nota sobre EnemyStatsSO:** La ruta real es `Scripts/AI/ScriptableObjects/`, no `Scripts/ScriptableObjects/` como se planificó originalmente. El campo `HP` del GDD (40 HP) **no está incluido** en el ScriptableObject actual. Tampoco incluye `contactDamage` ni `stunDuration`.

### 3.2 FSM — 3 estados

```
Idle (flotar, Speed = 0)
  ↓ rover en rango alertRange (5 u)
Alert (orientarse al rover, Speed = 0, timer alertTime)
  ↓ confirmación alertTime (0.5 s)
Chase (seguir al rover a moveSpeed, Speed = 1)
  ↓ rover fuera de rango loseRange (8 u) por loseTime (3 s)
Idle
```

**Estado:** ✅ Implementado según GDD. Usa `Vector2.MoveTowards()` para el movimiento y `transform.localScale` para el flip.

### 3.3 Valores del GDD vs Implementación actual

| Parámetro | Valor GDD | EnemyStatsSO actual | Estado |
|-----------|-----------|---------------------|--------|
| HP | 40 | ❌ No incluido | ⚠️ Agregar campo `hp` |
| `moveSpeed` base | 1.5 u/s | — | ⚠️ Solo un `moveSpeed` (sin diferenciar base/chase) |
| `moveSpeed` Chase | 3.75 u/s | `moveSpeed = 3.75f` | ✅ |
| Daño al contacto | 5 SI/s | ❌ No implementado | ⚠️ Pendiente (placeholder log o sistema de daño) |
| Rango detección | 5 u | `alertRange = 5f` | ✅ |
| Rango de pérdida | 8 u | `loseRange = 8f` | ✅ |
| Tiempo de pérdida | 3 s | `loseTime = 3f` | ✅ |
| Tiempo de alerta | 0.5 s | `alertTime = 0.5f` | ✅ |
| Vulnerabilidad | Aturdido 2 s (pulso escaneo) | ❌ No implementado | ⚠️ Pendiente |
| Comportamiento grupal | Individual | ✅ Correcto | ✅ |

### 3.4 Animator Controller

**Guardado en:** `Assets/Animations/Enemies/Ser Bioluminiscente/BiolAC.controller`

Sprites usados: `Enemy/Biol-idle-v1.png`, `Enemy/Biol-walk-v1.png`  
Sprite de ataque disponible: `Enemy/Biol-bio_attack-v1.png` ✨ (no integrado en animador aún)

| Estado | Sprite | Clip creado | Estado |
|--------|--------|-------------|--------|
| `BiolIdle` | `Biol-idle-v1.png` | `BiolIdle.anim` | ✅ Creado |
| `BiolWalk` | `Biol-walk-v1.png` | `BiolWalk.anim` | ✅ Creado |
| `BiolAttack` | `Biol-bio_attack-v1.png` | — | ❌ No creado (sprite disponible) |
| `Death` | ⬜ Por crear | — | ❌ No existe sprite ni clip |

### 3.5 Prefab

**Guardado en:** `Assets/Prefabs/Enemies/SerBioluminiscente.prefab` ✅

**Escena de prueba:** `Assets/Scenes/Enemy/BiolAC.unity` ✅

**Componentes verificados:**
```
Rigidbody2D
Collider2D
BioluminescentAI → usa EnemyStatsSO
Animator → BiolAC.controller
SpriteRenderer
```

### 3.6 Pendientes de la Tarea 3

- [ ] Agregar `hp`, `contactDamage`, `stunDuration` a `EnemyStatsSO.cs`
- [ ] Agregar `moveSpeedBase` (1.5 u/s) diferenciado de `moveSpeed` chase
- [ ] Implementar daño al contacto (log placeholder o sistema real)
- [ ] Implementar vulnerabilidad al pulso de escaneo (aturdimiento 2 s)
- [ ] Crear `BiolAttack.anim` usando `Biol-bio_attack-v1.png`
- [ ] Crear sprite y `BiolDeath.anim`
- [ ] Lógica de muerte (HP = 0 → animación Death → desactivar)

**Definition of Done:** El Ser Bioluminiscente patrulla en flotación. Al acercarse el rover a <5 u, se orienta y persigue. Al contacto, se registra un log o se reduce la SI (si el sistema ya está configurado). Al morir (HP = 0), reproduce animación Death y se desactiva.

---

## Tarea 4 — Audio: estados de música y Audio Mixer

**Rama git:** `feature/audio-system`  
**Responsable:** Technical Director  
**Ref. GDD:** §14 Audio  
**Estado global:** ❌ No iniciada (AudioManager sin cambios desde Sprint 01)

### 4.1 Mejoras al AudioManager.cs

**Estado actual:** Funcional pero básico. Solo `PlayMusic(clip, loop)`, `TriggerGameOverMusic()` y `PlayGlobalSFX(clip)`. Patrón singleton con `DontDestroyOnLoad`. Sin Audio Mixer, sin estados de música, sin crossfade.

**Mejoras necesarias:**

```csharp
// Agregar enum de estados
public enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }

// Agregar crossfade entre estados
// Exploration → Tension:  1.5 s
// Tension → Combat:       0.5 s
// Combat → Exploration:   3.0 s
// Cualquiera → Cinematic: gestionado por CinematicManager
```

### 4.2 Unity Audio Mixer — 4 buses

| Bus | Uso |
|-----|-----|
| Master | Volumen global |
| Music | Música adaptativa (crossfade entre clips) |
| SFX | Efectos del rover y enemigos |
| Ambient | Sonido ambiental de cueva |

### 4.3 Correcciones en PlayerAudioController

**Estado actual del PlayerAudioController:** Funcional con crossfade entre Idle/Walk usando 2 AudioSources (`loopAudioSource` + `oneShotAudioSource`). Incluye fade-out al despegar del suelo, `PlayJumpSound()`, `PlayDamageSound()`, `PlayDeathSound()`.

Correcciones pendientes:
- ⚠️ Corregir suscripción `OnDashed → PlayDamageSound` (línea 39) — crear `PlayDashSound()` o remover
- ❌ Agregar SFX de aterrizaje vía `OnLanded` (evento nuevo en PlayerController — **no existe aún**)
- ❌ Asegurar que los AudioSources usan el bus SFX del Mixer

### 4.4 SFX de enemigos

Cada enemigo necesita al mínimo:
- SFX de ataque (contacto para Biol)
- SFX de muerte

**Estado:** ❌ No existen archivos de SFX de enemigos en `Assets/Audio/SFX/`

**Definition of Done:** La música transiciona con crossfade al detectar/eliminar enemigos. El Audio Mixer permite ajustar Music, SFX y Ambient independientemente.

---

## Tarea 5 — Tilemap del Nivel 1 (Level Designer)

**Rama git:** `feature/level-tilemap`  
**Responsable:** Level Designer  
**Ref. GDD:** §9.2 Nivel 1, §16.1 Collision Matrix  
**Estado global:** 🔄 Parcial — herramientas de level design creadas, escena existe, tilemap pendiente de diseño

### 5.0 Herramientas de Level Design creadas ✅ NUEVO

| Herramienta | Archivo | Descripción |
|-------------|---------|-------------|
| `PlaceholderTileGenerator.cs` | `Scripts/Editor/` | Genera 5 tipos de tiles placeholder vía menú `Tools → Red Dust → Generate Placeholder Tiles` |
| `LevelMarker.cs` | `Scripts/Level/` | Marcadores visuales en Scene View para 7 tipos: SpawnPoint, Checkpoint, Scannable, BlockedZone, EnemyPatrol, BossRoom, EnergyCellPickup |
| `Placeholders_Palette.prefab` | `Art/Tiles/Placeholders/` | Palette de tiles para pintar en el Tilemap |

**Tiles placeholder disponibles:**

| Tile | Color | Uso |
|------|-------|-----|
| `Tile_Ground_Placeholder` | Gris oscuro | Suelo y paredes con colisión |
| `Tile_Platform_Placeholder` | Gris claro | Plataformas one-way |
| `Tile_Blocked_Placeholder` | Rojo | Zona Rueda Reforzada (30% inaccesible) |
| `Tile_Background_Placeholder` | Azul oscuro | Visual sin colisión |
| `Tile_Danger_Placeholder` | Naranja | Zonas de daño / fosos |

### 5.1 Configuración de Grid y Capas de Tilemap
- ✅ La escena `Level01.unity` existe en `Assets/Scenes/Level01/`
- ⬜ Crear o configurar GameObject principal `Grid`
- ⬜ Configurar `Collision_Tilemap` (Layer: `Ground`, usar `TilemapCollider2D` + `CompositeCollider2D`, Rigidbody2D `Static`, `Used by Composite`)
- ⬜ Configurar `Visual_Tilemap` (Layer: `Default`, sin colliders)

### 5.2 Diseño de Geometría por Zonas (Beat Map)
- **Zona 1 — Entrada (Tutorial implícito):**
  - Plataformas simples de piedra y suelo regular plano.
  - Diseñar el recorrido de manera que enseñe al jugador mecánicas básicas: movimiento lateral y salto.
  - Colocar un placeholder para el objeto escaneable `SC-01` (usar `LevelMarker` tipo `Scannable`).
- **Zona 2 — Primera tensión:**
  - Crear un foso/abismo con una anchura que exija el uso de Dash para cruzar con éxito.
  - Añadir una sala lateral elevada que resulte inaccesible por el momento (usar `LevelMarker` tipo `BlockedZone` + `Tile_Blocked_Placeholder`).
  - Espacio plano para patrullaje de enemigos (usar `LevelMarker` tipo `EnemyPatrol`).
  - Colocar `LevelMarker` tipo `Checkpoint` para el Checkpoint final de la zona.
- **Zona 3 — Respiro y Recurso (Sala del Meteorito):**
  - Crear una sala segura (sin spawns de enemigos).
  - Colocar `LevelMarker` tipo `Scannable` para `SC-02` y `LevelMarker` tipo `EnergyCellPickup` para upgrade del scanner.
- **Zona 4 — Escalada vertical (Sala del Leviatán):**
  - Diseñar una sección angosta y vertical para forzar el uso de Wall Jump en las paredes.
  - Colocar plataformas a diferentes alturas y `LevelMarker` tipo `Scannable` para `SC-03`.
  - Añadir `LevelMarker` tipo `Checkpoint` pre-boss.
- **Zona 5 — Boss (Leviatán):**
  - Sala amplia y horizontal, despejada de obstáculos terrestres (usar `LevelMarker` tipo `BossRoom`).

### 5.3 Bloqueo de Rutas e Integración de Prefabs
- Marcar el 30% del nivel como inaccesible utilizando `Tile_Blocked_Placeholder` + `LevelMarker` tipo `BlockedZone`, representando las zonas que requieren el upgrade `Rueda Reforzada`.
- Instanciar `Player.prefab` en el punto de spawn de la Zona 1 (usar `LevelMarker` tipo `SpawnPoint`).
- Instanciar `SerBioluminiscente.prefab` en zonas de patrullaje de enemigos.

**Definition of Done:** El nivel se puede recorrer de principio a fin (Zona 1 a Zona 4) usando el prefab del jugador sin caer al infinito ni atascarse en la geometría. La colisión con la capa `Ground` funciona de forma estable. Las zonas inaccesibles están claramente delimitadas con bloques provisionales.

---

## Configuración de Unity — Pendiente para Sprint 02

### Layers por configurar

| Layer | Uso | Estado |
|-------|-----|--------|
| Ground | Suelo y plataformas | ✅ Configurado |
| Player | GameObject Player | ⬜ Configurar |
| Enemy | Todos los enemigos | ⬜ Configurar |
| Platform | Plataformas one-way | ⬜ Configurar |
| Interactable | Objetos escaneables | ⬜ Configurar |

### Layer Collision Matrix (GDD §16.1)

Configurar en `Edit → Project Settings → Physics 2D → Layer Collision Matrix` según la tabla del GDD.

---

## Reglas del proyecto

- No agregar assets de Asset Store sin aprobación del Technical Director
- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin consultar al Gameplay Programmer
- No cambiar la resolución de referencia del Canvas (1920×1080)
- No cambiar la configuración de Physics 2D sin consultar al Technical Director
- Simulation Mode: `Fixed Update`
- Physics Material del Player y Ground: `NoFriction`
- Todo Rigidbody2D dinámico: `Collision Detection: Continuous`

---

## Flujo de trabajo Git

```
1. Antes de trabajar:
   git checkout feature/tu-rama
   git fetch origin
   git rebase origin/develop

2. Commits con Conventional Commits:
   feat(enemies): add Biol FSM with 3 states
   art(sprites): add Opportunity-dash spritesheet
   fix(audio): correct crossfade timing

3. Al terminar feature:
   git push origin feature/tu-rama
   Abrir Pull Request → base: develop
   Mínimo 1 approval antes de merge

4. NUNCA push directo a main o develop
5. SIEMPRE incluir archivos .meta en commits
6. NUNCA subir Library/ o Temp/
```

---

## Resumen de progreso por tarea

| Tarea | Responsable | Entregable clave | Prioridad | Progreso |
|-------|------------|------------------|-----------|----------|
| Correcciones Sprint 01 | Todos | Corregir suscripción OnDashed | 🔴 Bloqueante | ⚠️ Pendiente |
| T1 — Fondos/Parallax | Artist + TD | Fondos y Parallax en Level01 | 🔴 Alta | ❌ 0% — Sin fondos, sin ParallaxController |
| T2 — Animaciones rover | Artist + GP | RoverAC con estados MVP completos | 🔴 Alta | 🔄 ~40% — Dash sprite ✅, falta integración + 4 sprites |
| T3 — Ser Bioluminiscente | AI Programmer | Prefab con FSM 3 estados + daño | 🟡 Media | 🔄 ~70% — FSM ✅, Prefab ✅, Animator ✅, falta HP/daño/muerte |
| T4 — Audio Mixer | Technical Director | Crossfade estados música + Mixer | 🟡 Media | ❌ 0% — AudioManager sin cambios |
| T5 — Tilemap del Nivel 1 | Level Designer | Level01 con geometría jugable | 🔴 Alta | 🔄 ~30% — Herramientas ✅, escena ✅, geometría no pintada |

---

## Resumen de cambios desde la versión anterior del Sprint 02

> Registro de actualizaciones para trazabilidad.

### Cambios detectados (2026-06-18)

**Nuevos archivos creados:**
- `Assets/Scripts/AI/BioluminescentAI.cs` — FSM de 3 estados para el Ser Bioluminiscente
- `Assets/Scripts/AI/ScriptableObjects/EnemyStatsSO.cs` — ScriptableObject base para stats de enemigos
- `Assets/Scripts/AI/ScriptableObjects/BiolStats.asset` — Instancia con valores
- `Assets/Scripts/Level/LevelMarker.cs` — Marcadores visuales de Scene View (7 tipos)
- `Assets/Scripts/Editor/PlaceholderTileGenerator.cs` — Generador de tiles placeholder
- `Assets/Art/Tiles/Placeholders/` — 5 tiles + palette
- `Assets/Animations/Enemies/Ser Bioluminiscente/` — BiolAC.controller + 2 clips (Idle, Walk)
- `Assets/Prefabs/Enemies/SerBioluminiscente.prefab` — Prefab completo del enemigo
- `Assets/Art/Sprites/Oppy/Opportunity-dash-v1.png` y `dash-v3.png` — Sprites de dash del rover
- `Assets/Art/Sprites/Enemy/Biol-bio_attack-v1.png` — Sprite de ataque del Biol
- `Assets/Scenes/Level01/Level01.unity` — Escena del Nivel 1
- `Assets/Scenes/Level02/Level02.unity` — Escena del Nivel 2 (placeholder)
- `Assets/Scenes/Enemy/BiolAC.unity` — Escena de prueba del Biol

**Reorganización de carpetas:**
- Sprites del rover: de `Assets/Art/Sprites/` → `Assets/Art/Sprites/Oppy/`
- Sprites del Biol: de `Assets/Art/Sprites/` → `Assets/Art/Sprites/Enemy/`
- Audio: de `Assets/Audio/` plano → `Assets/Audio/Music/` y `Assets/Audio/SFX/`
- Se eliminaron sprites de concept art de enemigos (archivos base sin sufijo de animación)

**Scripts sin cambios respecto al Sprint 01:**
- `PlayerController.cs` — Sin modificaciones
- `PlayerAnimatorController.cs` — Sin modificaciones
- `PlayerAudioController.cs` — Sin modificaciones (bug OnDashed persiste)
- `AudioManager.cs` — Sin modificaciones
- `RoverStatsSO.cs` — Sin modificaciones
- `SceneLoader.cs` — Sin modificaciones
- `MainMenuController.cs` — Sin modificaciones