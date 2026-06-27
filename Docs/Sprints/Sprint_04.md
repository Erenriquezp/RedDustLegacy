# Sprint 04 — Pantallas del sistema, Nivel 1 completo, Boss Leviatán y audio integral

> **Estado:** ⬜ Pendiente· **Creado:** 2026-06-23 · **Prerequisito:** Sprint 03 · **Ref. GDD:** §8.3, §9.2, §10, §12.2, §14, §15.1 · **Ref. HUD:** §3, §4.1, §6, §10

Cierre del vertical slice del Nivel 1: las tres pantallas de sistema (menú principal, carga, pausa), el diseño completo del Nivel 1 (Z1→boss), el Boss Leviatán (animación + lógica) y la capa de audio de todo el nivel (pantallas, botones, enemigos, ambiente y música adaptativa).

## Equipo y ramas

| Rol | Rama | Tareas en este sprint |
|-----|------|-----------------------|
| Technical Director (Líder) | `feature/ui-screens` / `feature/audio-system` | T1 (pantallas), T4 (audio) |
| Level Designer | `feature/level-design` | T2 (Nivel 1 completo, colocación) |
| AI Programmer | `feature/enemy-ai` | T3 (lógica del Leviatán) |
| Artist | arte | T1 (arte de pantallas), T3 (sprites/animación del Leviatán), T4 (assets SFX) |
| Gameplay Programmer | `feature/player-movement` | apoyo T3 (vector de daño del rover al núcleo) |

## Orden recomendado

```
T2 Nivel 1 ──────────────┬─→ T3 Leviatán   (arena, lockdown, spawn)
(consolida Level01.unity) ├─→ T4 Audio       (ambiente y triggers por zona)
                          └─→ T1 Pantallas    (carga real apunta a Level01)
T3 Leviatán ──────────────→ T4 Audio          (SFX/música de combate del boss)
```

---

## T1 — Pantallas del sistema (menú principal, carga, pausa)
**Responsable:** Technical Director (+ Artist) · **Rama:** `feature/ui-screens` · **Ref. GDD:** §10, §12.2 · **Ref. HUD:** §10.1–10.3

**Estado actual del código:** `MainMenuController` solo tiene `OnPlayPressed`/`OnQuitPressed`; `SceneLoader` carga **síncrono** (`SceneManager.LoadScene`), así que **no hay pantalla de carga real**. Hay que ampliar ambas y crear los Canvas de cada pantalla. Todas a **referencia 1920×1080, Screen Space Overlay, Scale With Screen Size**; estética terminal espacial diegética (HUD §10).

### 1.1 Pantalla de inicio / Menú principal (HUD §10.1)
Rediseñar el `MainMenu.unity` con layout asimétrico (UI a la izquierda, ilustración del rover a la derecha).

| Opción | Acción (callback) | Subtítulo / color | Estado en prototipo |
|--------|-------------------|-------------------|---------------------|
| **CONTINUAR** | cargar último checkpoint (placeholder → Level01) | `SOL 5847 - SEÑAL CON TIERRA: PERDIDA` ámbar `#FFD74A` | Funcional mínimo (carga Level01) |
| **NUEVA MISIÓN** | `SceneLoader.LoadLevel01()` (vía pantalla de carga) | `INICIAR UNA NUEVA EXPLORACIÓN` gris `#8A9BA8` | Funcional |
| **ARCHIVO DE MISIÓN** | abrir Log Screen (HUD §10.4) | `REGISTROS Y TRANSMISIONES` | Botón presente, deshabilitado (V2) |
| **OPCIONES** | abrir panel de opciones (audio/controles) | `CONFIGURACIÓN DEL SISTEMA` | Volumen Music/SFX (engancha T4) |
| **SALIR** | `Application.Quit()` | `CERRAR SESIÓN DEL SISTEMA` | Funcional (ya existe) |

- Logo `OPPORTUNITY` (esténcil) + `RED DUST LEGACY` (`#E07040`); firma `NASA • MARS EXPLORATION ROVER • MER-B`.
- Cursor de selección: corchete `>` parpadeante a la izquierda de la opción activa (navegable con teclado/gamepad, no solo ratón).
- Panel "Última transmisión" (inferior-izq, marco `#3A8FC1`): `Houston... si alguien puede oirme... mi energia se agota... pero seguire adelante. SOL 5847`.
- Leyendas de control: `[Navegar]` `ENTER [Seleccionar]` `ESC [Atrás]`.

### 1.2 Pantalla de carga (HUD §10.2, GDD §12.2)
Crear `Scripts/UI/LoadingScreenController.cs` + escena/Canvas `Loading`. **Cambio técnico obligatorio:** migrar `SceneLoader` a **carga asíncrona**.

```csharp
// SceneLoader.cs — añadir
public void LoadLevelAsync(string sceneName) => StartCoroutine(LoadRoutine(sceneName));
// LoadRoutine: activar Canvas Loading → AsyncOperation op = SceneManager.LoadSceneAsync(name);
// op.allowSceneActivation = false; actualizar barra con op.progress (0–0.9 → normalizar a 0–1);
// al llegar a 0.9 y mínimo de tiempo cumplido → allowSceneActivation = true.
```

Maquetar la terminal JPL con los valores **fijos del GDD §12.2 / HUD §10.2** (son datos diegéticos, no de gameplay):

| Sistema vital | Valor | Color barra |
|---------------|-------|-------------|
| POTENCIA | 78% | verde |
| COMUNICACIONES | 91% | verde |
| SENSORES | 76% | ámbar |
| MOVILIDAD | 82% | verde |
| INTEGRIDAD ESTRUCTURAL | **74%** | naranja |

- Cabecera: `CARGANDO -- NIVEL 1 - LAS CUEVAS BIOLUMINISCENTES` (naranja) + `REGISTRO DEL SISTEMA / TELEMETRÍA EN TIEMPO REAL`; esquina `OPPORTUNITY // MER-B` + `• REC TELEMETRÍA` (punto rojo parpadeante).
- Ficha hardware: `MODO: EXPLORACIÓN AUTÓNOMA`, `ESTADO: NOMINAL` (LED verde), `BATERÍA: 78%`, `TEMP: -18.7 °C`, `TIEMPO EN MISIÓN: 05:31:42:17`.
- Estado de misión: SISTEMAS/CALIBRACIÓN/DESPLIEGUE = `COMPLETADO`; EXPLORACIÓN = `EN CURSO`; TRANSMISIÓN = `PENDIENTE`.
- Ubicación: `MARTE — ARCADIA PLANITIA`, `10.2° N, 112.4° E`, `ELEVACIÓN -4.3 km` (globo en rotación lenta).
- Barra de progreso: franjas diagonales naranja/negro animadas + `%` real de `AsyncOperation` + silueta del rover avanzando con el progreso. **Tiempo mínimo en pantalla: 2 s** aunque la carga sea instantánea (que la telemetría se lea).

### 1.3 Pantalla de pausa (HUD §10.3)
`PausePanel` dentro del Canvas de Level01 (no escena propia), desactivado por defecto. **La lógica de `timeScale = 0` y el toggle la maneja `GameManager.TogglePause()` (S03 T5); este panel solo presenta y enruta botones.**

- Efecto de fondo: blur + tinte negro 60% + cuadrícula gris fina sobre la pantalla congelada; marco cian con esquineras.
- Wireframes técnicos del chasis en los laterales (arte estático).

| Opción | Callback | Notas |
|--------|----------|-------|
| **REANUDAR DIAGNÓSTICO** | `GameManager.ResumeGame()` | timeScale → 1 |
| **REINICIAR DESDE CHECKPOINT** | `GameManager.RestartFromCheckpoint()` | usa `CheckpointManager` (S03 T4) |
| **SISTEMA DE CONFIGURACIÓN** | abrir opciones (volúmenes T4, controles) | comparte panel con menú principal |
| **ARCHIVO DE TRANSMISIÓN** | abrir Log Screen | deshabilitado si Log es V2 |
| **ABANDONAR SESIÓN** | `SceneLoader.LoadMainMenu()` | restaurar timeScale → 1 antes de cargar |

**DoD T1:** desde el menú, "Nueva Misión" muestra la pantalla de carga con barra real (`AsyncOperation`) ≥2 s y entra a Level01; `Esc` en juego abre la pausa con `timeScale 0` y las 5 opciones enrutan correctamente (Reanudar, Reiniciar, Config, Archivo, Abandonar); la navegación funciona con teclado/gamepad, no solo ratón.

---

## T2 — Nivel 1 completo (diseño de nivel)
**Responsable:** Level Designer · **Rama:** `feature/level-design` · **Ref. GDD:** §9.2, §16.1 · **Ref. HUD:** §4.1

Continuación directa del Sprint 02 T5. La **base ya está montada** en `Level01.unity` (geometría consolidada, sistemas core cableados y probados); falta **poblar y completar** el recorrido íntegro Sala de Reinicio → Arena del Leviatán según el timeline del HUD §4.1.2.

### Estado actual de `Level01.unity` (2026-06-26)

**✅ Ya en la escena:**
- Geometría: `Grid` + 6 tilemaps (`Collision`, `Visual`, `OneWay`, `Danger`, `Markers`, `Front`); `Danger_Tilemap` en layer `Ground` con `HazardDamage`.
- Sistemas: HUD completo (barra SI, celdas, alertas, paneles Pausa/GameOver) + `EventSystem`, `CheckpointManager`, `BackgroundMusic`, `Global Light 2D`, `CinemachineCamera`.
- Jugable: Player, **1 Biol** y **1 Drone** (terrestre) funcionando; **4 checkpoints** colocados; hazards (≈16 cristales, obstáculos giratorios, `PlataformaMovil`, `Caida*`); plataformas flotantes.
- Organizado en contenedores: `Systems`, `Platforms`, `Hazards`, `Enemies`, `Markers`.
- Build Settings: `MainMenu` → `Level01` → `Level02`.

**🔴 Falta para el "Nivel 1 completo":**
- **Poblar enemigos:** subir a **6 Biol + 2 Drones** (hoy 1 + 1) y distribuirlos por zonas según el presupuesto de daño.
- **Escaneables SC-01/02/03** + el **sistema de escaneo** (no existe en la escena; el rover ya tiene el input `IsScanning`, pero no hay objetos ni lógica de escaneo/lore).
- **Marcadores de zona:** solo existe `SpawnPoint` (Z1). Faltan Z2–Z5, `BlockedZone` y `BossRoom`.
- **Upgrades:** Rueda Reforzada (Z2) y Escaneo Mejorado (Z3) — sin pickups ni gating.
- **Arena del Leviatán** (lockdown + spawn → T3) y **trigger de cinemática/`LevelExit`** al derrotar al boss.
- **Config menor:** la instancia `SerBioluminiscente` tiene un override de tag `Player` equivocado; corregir.

> ✅ **Parallax del fondo: hecho** (S02 T1, cerrado 2026-06-27) — `Background` con `ParallaxMovement` (scroll por offset de textura), centrado/cobertura/orden corregidos.

### Métricas objetivo del Nivel 1 (GDD §9.2)
| Parámetro | Valor |
|-----------|-------|
| Bioma | Cueva de cristal — luz azul-verde ambiental |
| Duración estimada | 25–35 min |
| Checkpoints | **3** (inicio, media progresión, pre-boss) |
| Enemigos | Ser Bioluminiscente **×6**, Drone Patrullero **×2**, Leviatán **×1** (boss) |
| Objetos escaneables | SC-01, SC-02, SC-03 |
| Upgrades | Rueda Reforzada (Zona 2, opcional), Escaneo Mejorado (Zona 3) |
| SI al inicio | **74%** (Fase 1) |
| SI estimada al final | **47–61%** (define el presupuesto de daño del nivel) |

### Beat map completo (HUD §4.1.2)
Cada segmento se pinta sobre `Collision_Tilemap` (layer `Ground`) salvo plataformas one-way (`OneWay_Tilemap`, layer `Platform`). Marcar con `LevelMarker` (hoy solo está `SpawnPoint`/Z1; faltan el resto).

| # | Segmento | Contenido jugable | Marcadores / contenido |
|---|----------|-------------------|------------------------|
| 0 | Sala de Reinicio | arranque en negro, faro se enciende; suelo plano corto | `SpawnPoint` |
| 1 | Galería de Cristal (Z1) | tutorial correr+saltar; 2 Biol en plano medio | `Scannable` SC-01, `EnemyPatrol` ×2 (Biol) |
| 2 | Bifurcación A (opcional, vertical) | pozo de **wall-jump**; al tope, upgrade + **Checkpoint 1** | `Checkpoint`, upgrade Rueda Reforzada |
| 3 | Caverna Central (Z2) | cámara amplia, estanques; meteorito SC-02 | `Scannable` SC-02, `EnergyCellPickup`, `BlockedZone` (exige Rueda Reforzada) |
| 4 | Sala del Meteorito (Z3) | desfiladero estrecho; introduce **Drone Patrullero ×2** | `EnemyPatrol` (Drone), upgrade Escaneo Mejorado, `Scannable` SC-03 |
| 5 | Corredor de Arcilla (Z4) | grieta vertical wall-jump; Sporex; **Geysers**; **plataformas móviles** | `Checkpoint` 2, `PlataformaMovil`, hazards |
| 6 | Sala Pre-Boss | cámara silenciosa, silueta del Leviatán tras el cristal | **Checkpoint 3**, `EnergyCellPickup` |
| 7 | Arena del Leviatán | sala circular cerrada de basalto; lockdown al entrar | `BossRoom` (spawn del boss → T3) |
| 8 | Cinemática de caída | trigger al derrotar al boss → transición a N2 | `LevelExit` / trigger de cinemática |

### Reglas de colocación y dificultad
- **Presupuesto de daño:** el recorrido debe dejar al rover en **47–61% SI** al llegar al boss (≈13–27 SI de daño acumulado desde 74%). Distribuir contacto Biol (5 SI/s) y proyectiles drone (8–12 SI) en consecuencia; no saturar antes del boss.
- **Mecánicas de nivel ya disponibles** (`Scripts/leveo01/`, ver Sprint 02): `PlataformaMovil`, `GiroCompleto`, `OsciladorGiro` (en uso); colocar por fin `CaidaCristal`/`CaidaPorCercania` como trampas en Z3/Z4 (asignar `capaObjetivo = Player`).
- **Checkpoints ≥8 u del enemigo más cercano** (GDD §7).
- **Zonas bloqueadas** (`BlockedZone` + tiles bloqueados) que exigen Rueda Reforzada en ~Z2.
- Cerrar bordes (techo/paredes) en `Collision_Tilemap`; cámara `CinemachineCamera.Follow = Player` + `Confiner 2D` por sala.
- Asignar el campo `rover` de cada `BioluminescentAI`/Drone al Player de la escena (si no, `NullReferenceException`).

**DoD T2:** se recorre Sala de Reinicio → Arena del Leviatán sin caer al vacío ni atascarse; los 3 checkpoints registran; los 6 Biol + 2 Drones están colocados y persiguen; SC-01/02/03 son escaneables; al entrar a la arena se dispara el lockdown y el spawn del boss (T3); el rover llega al boss con SI dentro de 47–61%.

---

## T3 — Boss Nivel 1: Leviatán (animación + lógica completa)
**Responsable:** AI Programmer (lógica) + Artist (animación) + Gameplay (vector de daño) · **Rama:** `feature/enemy-ai` · **Ref. GDD:** §8.3 · **Ref. HUD:** §2, §3, §4.1.2(8)

**Estado actual:** no existe ningún script ni controller del Leviatán. Crear `Scripts/AI/LeviatanAI.cs` y `Scripts/AI/LeviatanCore.cs` (hitbox del punto débil), `DronePatrollerStats`-style `LeviatanStatsSO` (o ampliar `EnemyStatsSO`) y `LeviatanAC.controller`.

### Parámetros (GDD §8.3)
| Parámetro | Valor |
|-----------|-------|
| HP | **200** |
| `moveSpeed` | 0,8 u/s |
| Daño tentáculo | **15 SI** — i-frames **0,8 s** |
| Patrón de ataque | **3 tentáculos alternos**, **2 s** de pausa entre ataques |
| Punto débil | **Núcleo central — ×2 daño**. Solo vulnerable durante la pausa post-ataque |
| Enfurecimiento | **<50% HP** (<100): velocidad tentáculos ×1,5 + **4º tentáculo** al ciclo |
| Invulnerabilidad | Tentáculos **no** reciben daño — **solo el núcleo** |
| Proyectiles | **No** — solo ataques de contacto |
| Escala / sprite | Cuerpo 96×64 px, **×3.0** respecto al rover (HUD §2) |

### ✅ Vector de daño del rover — resuelto (dash ofensivo)
El rover no tiene melee; daña a los enemigos **embistiéndolos con dash** (`PlayerController.IsDashing`). Ya está implementado y probado en S03 para Biol y Drone (cada uno con su `dashDamage`, 1 golpe por dash con cooldown). **Reusar el mismo patrón para el `LeviatanCore`:** trigger en el núcleo + `OnTrigger/CollisionStay2D` → si el rover llega con dash y el núcleo está expuesto, restar HP con el multiplicador ×2.
- Fijar `dashDamage` del núcleo: sugerido 25 (×2 = 50) ⇒ ~4 ventanas para 200 HP.
- Los **i-frames** del rover ya existen (`DegradationSystem.invulnDuration`); el tentáculo del boss se beneficia de ellos. El GDD pide 0,8 s para el boss (hoy global 0,6 s) — subirlo o gestionarlo por fuente si hace falta.

### FSM y máquina de ataque
```
Intro (lockdown + emerge del estanque)
  → Idle/Track (orienta hacia el rover, moveSpeed 0,8)
      → Attack (3 tentáculos alternos; 4 si enrage)
          → Vulnerable (pausa 2 s, núcleo expuesto → recibe daño)
              → [HP < 50%] Enrage (tentáculos ×1,5 + 4º) ─┐
              └────────────→ Idle/Track ←─────────────────┘
  → Death (HP ≤ 0 → animación, desbloquea salida, dispara cinemática de caída)
```
- `LeviatanCore` (hijo con `Collider2D` trigger): activo/visible **solo** en estado `Vulnerable`; recibe el **dash** del rover (mismo patrón que Biol/Drone) y resta al HP del boss con el multiplicador ×2.
- Tentáculo: `Collider2D` que en impacto llama `rover.GetComponentInParent<DegradationSystem>().TakeDamage(15, transform.position)`. Los **i-frames** (0,6 s) y el knockback ya los aplica `DegradationSystem` de forma central — no hay que reimplementarlos (subir a 0,8 s si el diseño del boss lo exige).
- Barra de vida del boss en el HUD (delegar en `HUDManager`); aparece en `Intro`, desaparece en `Death`.

### Animaciones del Leviatán (`LeviatanAC`) — Artist
HUD §3 no tabula el Leviatán explícitamente; definir estos clips a partir del comportamiento (núcleo expuesto intermitente, tentáculos oscilantes, HUD §4.1.2 punto 8):

| Estado | Loop | Contexto visual |
|--------|:---:|-----------------|
| `Idle` | Sí | masa central pulsa; tentáculos oscilan suaves; núcleo **cerrado** |
| `Attack` | No | golpe de tentáculos alternos (3, o 4 en enrage) |
| `Vulnerable` | Sí (corto) | núcleo **expuesto** brillando naranja (contraste con arena azul) |
| `Enrage` | Sí | flujo errático de partículas; ritmo de tentáculos ×1,5 |
| `Death` | No | colapso, apagado de brillo, disolución |

Params del Animator: `IsAttacking` (Bool), `IsVulnerable` (Bool), `IsEnraged` (Bool), `IsDead` (Bool), `AttackTrigger` (Trigger).

**DoD T3:** el Leviatán emerge con el lockdown de la arena; ataca con 3 tentáculos alternos (15 SI + i-frames 0,8 s) y 2 s de pausa; el núcleo solo es dañable en la pausa y aplica ×2; a <50% HP entra en enrage (velocidad ×1,5 + 4º tentáculo); los tentáculos son invulnerables; HP=0 reproduce Death, desbloquea la salida y dispara la cinemática de caída a Nivel 2.

---

## T4 — Audio integral del Nivel 1 (pantallas, botones, enemigos, ambiente, música)
**Responsable:** Technical Director (+ Artist para assets) · **Rama:** `feature/audio-system` · **Ref. GDD:** §14, §15.1 · **Ref. HUD:** §10

**Estado actual (2026-06-26):** `AudioManager` solo expone `PlayMusic`/`TriggerGameOverMusic`/`PlayGlobalSFX`; **no existe `.mixer`** ni `enum MusicState`/`SetMusicState` (arrastre del Sprint 02 T4, núcleo 0%). **Sí** existe ya buena parte de la **capa SFX** que este sprint planificaba (adelantada en Sprint 02 / `feature/audio-system`):
- ✅ **4.4 SFX enemigos (Biol):** `EnemyAudioController` con loop 3D + ataque/daño/muerte, disparado desde `BioluminescentAI`.
- ✅ **4.5 SFX rover:** `sfxDash`/`PlayDashSound` (bug S01 ya corregido), `PlayDamageSound(fase)` con pitch −5%/fase, landing y death.
- 🟡 **4.3 SFX UI (parcial):** `UIAudioController` con `hover`/`click` (faltan `ui_back`, `ui_pause_open/close`, `ui_loading_tick`).

Este sprint cierra el **núcleo pendiente** (Mixer + `MusicState`) y completa el SFX de Drone/Leviatán y el ambiente.

### 4.1 Audio Mixer (cerrar deuda S02 T4)
Crear `Assets/Audio/MainMixer.mixer` con **Master → {Music, SFX, Ambient, UI}** y exponer `MusicVol`, `SfxVol`, `AmbientVol`, `UiVol` (los engancha el panel de Opciones de T1). Enrutar:
- `AudioManager.musicSource` → **Music**; `PlayerAudioController` (loop/oneShot) → **SFX**; ambiente de nivel → **Ambient**; clics de UI → **UI**.

### 4.2 Estados de música adaptativa (GDD §14.1)
`AudioManager`: `enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }` + `SetMusicState(state)` con crossfade.

| Estado | Condición de disparo | Crossfade |
|--------|----------------------|-----------|
| `EXPLORATION` | SI ≥41% y sin enemigos detectados | 1,5 s |
| `TENSION` | algún enemigo en Alert/Chase, **o** SI 20–40% | 0,8 s |
| `COMBAT` | **Boss activo** o múltiples enemigos en Chase | 0,5 s |
| `CINEMATIC` | cinemáticas/flashbacks/final | gestionado por cinemática |

Disparadores en Nivel 1: la IA del Biol/Drone notifica Alert/Chase → `TENSION`; el `LeviatanAI.Intro` → `COMBAT`; al morir el boss → `CINEMATIC`.

### 4.3 SFX de UI (todas las pantallas y botones) — T1
Set mínimo por el bus **UI**: `ui_navigate` (mover cursor), `ui_select` (confirmar), `ui_back` (Esc/atrás), `ui_pause_open`/`ui_pause_close`, `ui_loading_tick` (telemetría de la pantalla de carga). Disparar desde los `EventTrigger`/callbacks de cada botón en Menú, Pausa, Carga (y Game Over de S03).

### 4.4 SFX de enemigos — bus SFX
| Enemigo | Eventos a sonorizar | Disparo |
|---------|--------------------|---------|
| Ser Bioluminiscente | ataque (`Attack`), daño por contacto, **muerte** (`Death`, S02 T3) | desde `BioluminescentAI` |
| Drone Patrullero | propulsor en patrulla (loop), disparo, impacto proyectil, muerte | desde `DronePatrollerAI`/`EnemyProjectile` (S03 T3) |
| **Leviatán** | rugido al emerger, golpe de tentáculo, **impacto en el núcleo**, enrage, muerte | desde `LeviatanAI` (T3) |

### 4.5 SFX del rover (GDD §14.2) — cerrar pendientes
- Arreglar el **bug S01**: en `PlayerAudioController` cambiar `OnDashed += PlayDamageSound` por un `sfxDash` propio (`PlayDashSound`). Enganchar `onDamageReceived` (S03) a `PlayDamageSound` con **pitch −5% por fase de degradación**.

### 4.6 Ambiente del Nivel 1 — bus Ambient
Capas de ambiente por zona (HUD §4.1): viento/goteo de cueva (base), géiseres turquesa (Z4, pulsos), zumbido de cristal al escanear, silencio/zumbido en Sala Pre-Boss. Loop espacial, volumen relativo por proximidad.

**DoD T4:** el Mixer permite ajustar Music/SFX/Ambient/UI por separado y los volúmenes responden al panel de Opciones; la música transiciona con crossfade Exploration↔Tension↔Combat según enemigos/boss; cada botón de las pantallas suena; Biol, Drone y Leviatán emiten sus SFX clave; el bug `OnDashed→PlayDamageSound` queda corregido; el ambiente de cueva suena en Level01.

---

## Reglas del proyecto

- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin avisar al Gameplay Programmer (T1 sí toca `SceneLoader` para la carga async: coordinar). No tocar el Canvas de referencia 1920×1080 sin el Technical Director.
- Conventional Commits; rebase sobre `develop` antes del PR; nunca push directo a `main`/`develop`; **siempre** incluir `.meta`; nunca subir `Library/`.

## Progreso

> Sprint creado el 2026-06-23. Revisión 2026-06-26.

| Tarea | Responsable | Prioridad | Estado |
|-------|-------------|-----------|--------|
| T1 — Pantallas (menú/carga/pausa) | Technical Director | 🔴 Alta | ⬜ 0% |
| T2 — Nivel 1 completo | Level Designer | 🔴 Alta | 🟡 Base montada y probada (geometría + sistemas core + 1 Biol/1 Drone + 4 checkpoints); parallax ✅; falta poblar enemigos (6+2), escaneables, zonas Z2–Z5, upgrades, arena del boss y cinemática |
| T3 — Boss Leviatán | AI Programmer | 🔴 Alta | ⬜ 0% (sin script ni controller; **vector de daño ya resuelto:** dash ofensivo) |
| T4 — Audio integral | Technical Director | 🟡 Media | 🟡 ~30% (capa SFX Biol/rover ✅ y UI parcial; **núcleo Mixer + `MusicState` aún 0%**; falta SFX Drone/Leviatán y ambiente) |
