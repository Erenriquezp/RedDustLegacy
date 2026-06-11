# Sprint 01 — MVP Base

> **Objetivo:** Movimiento completo del rover, menú funcional, estructura de escenas y audio base.  
> **Estado:** ✅ Completado  
> **Ref. GDD:** §2 Controles, §3 Mecánicas del jugador, §14 Audio

---

## 1. Estructura del proyecto

```
Assets/
├── Animations/Rover/
│   ├── RoverAC.controller          ← Animator Controller principal
│   ├── Rover_Idle.anim             ← 25 frames, 12 fps, loop ☑
│   ├── Rover_walk.anim             ← 25 frames, 16 fps, loop ☑
│   ├── Rover_Run.anim              ← pendiente de integración
│   ├── Rover_Jump.anim             ← 25 frames, 20 fps, loop ☐
│   └── Rover_Scan.anim             ← pendiente de integración
├── Art/
│   ├── Backgrounds/
│   └── Sprites/                    ← Spritesheets cortados del rover
├── Audio/
│   ├── Music/                      ← BGM: main theme, game over
│   └── SFX/                        ← SFX: idle, walk, jump, damage, death
├── Input/
│   └── RoverInputActions_Local.inputactions
├── Scenes/
│   ├── Dev/Dev_PlayerMovement.unity  ← sandbox de pruebas de movimiento
│   ├── MainMenu/MainMenu.unity
│   ├── Level01/Level01.unity         ← escena de juego principal
│   └── Level02/Level02.unity         ← creada, pendiente de contenido
├── Scripts/
│   ├── Core/
│   │   ├── SceneLoader.cs            ← Singleton, DontDestroyOnLoad
│   │   └── AudioManager.cs           ← Singleton, música + SFX globales
│   ├── Player/
│   │   ├── PlayerController.cs       ← Movimiento completo del rover
│   │   ├── PlayerAnimatorController.cs ← Bridge animator desacoplado
│   │   └── PlayerAudioController.cs  ← SFX del rover con crossfade
│   ├── ScriptableObjects/
│   │   ├── RoverStatsSO.cs           ← Definición de parámetros
│   │   └── RoverStats_Default.asset  ← Valores tuneados en Inspector
│   ├── UI/
│   │   └── MainMenuController.cs     ← Play / Quit
│   ├── AI/                           ← vacío — Sprint 2
│   ├── Audio/                        ← vacío — Sprint 2
│   └── Level/                        ← vacío — Sprint 2
├── Settings/
│   ├── Physics/NoFriction.physicsMaterial2D  ← Friction: 0, Bounciness: 0
│   ├── UniversalRP.asset
│   └── Renderer2D.asset
└── TextMesh Pro/                     ← TMP para UI
```

---

## 2. Escenas

### 2.1 MainMenu

**Hierarchy:**
```
MainMenu
├── Main Camera              (Orthographic, Z: -10)
├── StarParticles            (Particle System 3D, Z: 10)
│     ├── Material:          ParticlesUnlit
│     ├── Shape:             Box 15×1×8
│     └── Color:             #AFA9EC
├── Canvas                   (Screen Space - Camera)
│     ├── Background         (Image #0A0A1A, full screen)
│     └── CenterGroup        (Vertical Layout Group)
│           ├── Subtitle     "A GAME BY RED DUST TEAM"
│           ├── Title        "RED DUST LEGACY"
│           ├── Divider      (línea horizontal)
│           ├── Tagline      "Survive. Explore. Adapt."
│           ├── PlayButton   → SceneLoader.LoadLevel01()
│           └── QuitButton   → Application.Quit()
└── SceneLoader              (DontDestroyOnLoad)
```

**Scripts:**

| Script | Ruta | Función |
|--------|------|---------|
| `MainMenuController` | `Scripts/UI/` | `OnPlayPressed()` → carga Level01. `OnQuitPressed()` → cierra app |
| `SceneLoader` | `Scripts/Core/` | Singleton. `LoadMainMenu()`, `LoadLevel01()`, `LoadLevel02()`, `LoadNextLevel()`, `ReloadCurrentScene()` |

### 2.2 Level01

**Hierarchy:**
```
Level01
├── Main Camera
├── Global Light 2D
├── CinemachineCamera          (sigue al Player)
├── Ground                     (BoxCollider2D, Layer: Ground, Material: NoFriction)
└── Player
      ├── Sprite               (SpriteRenderer + Animator → RoverAC)
      ├── GroundCheckPoint     (posición Y: -0.6)
      └── WallCheckPoint       (posición X: ±0.5)
```

**Componentes del Player:**

```
Rigidbody2D
  ├── Gravity Scale:           0         ← gravedad manual en PlayerController
  ├── Collision Detection:     Continuous
  ├── Interpolate:             None
  ├── Freeze Rotation Z:       ☑
  └── Inertia:                 1         ← forzado en Start()

CapsuleCollider2D
  ├── Size:                    X: 1.8  Y: 0.8
  ├── Direction:               Vertical
  └── Material:                NoFriction

PlayerController
  ├── Stats:                   RoverStats_Default
  ├── Ground Check Point:      GroundCheckPoint
  ├── Ground Check Size:       (0.7, 0.05)
  ├── Ground Layer:            Ground
  ├── Wall Check Point:        WallCheckPoint
  ├── Wall Check Size:         (0.05, 0.8)
  └── Wall Layer:              Ground

PlayerInput
  ├── Actions:                 RoverInputActions_Local
  ├── Default Map:             Player
  ├── Behavior:                Invoke Unity Events
  └── Events:
        ├── Jump →             PlayerController.OnJumpInput
        ├── Dash →             PlayerController.OnDashInput
        └── Scan →             PlayerController.OnScanInput

PlayerAnimatorController
  └── Lee Animator desde hijos (Sprite)

PlayerAudioController
  ├── sfxIdle, sfxWalk, sfxJump, sfxDamage, sfxDeath  (AudioClips)
  ├── loopAudioSource          ← motor del rover (crossfade idle↔walk)
  └── oneShotAudioSource       ← one-shots (jump, damage, death)
```

### 2.3 Dev_PlayerMovement

Escena sandbox para pruebas de movimiento aisladas del contenido del nivel. No se incluye en builds.

### 2.4 Level02

Escena creada, sin contenido. Reservada para Sprint 2+.

---

## 3. Sistemas implementados

### 3.1 PlayerController — Movimiento completo

> **Documento detallado:** [PlayerController.md](file:///c:/Users/LENOVO/RedDustLegacy/Docs/Architecture/PlayerController.md)

- ✅ Carrera con aceleración/desaceleración diferenciada suelo/aire
- ✅ Gravedad manual (`gravityScale = 0` en Rigidbody2D) con caída más pesada que subida
- ✅ Salto variable (jump cut al soltar el botón)
- ✅ Coyote time (0.08 s en asset actual)
- ✅ Jump buffer (0.1 s)
- ✅ Dash con freeze frames (`dashSleepTime`) y `gravity = 0` durante dash
- ✅ Un dash aéreo por salto, se restaura al aterrizar
- ✅ Wall slide con velocidad limitada (−2 u/s)
- ✅ Wall jump con lock de input horizontal (0.15 s)
- ✅ Escaneo: detiene movimiento horizontal mientras `isScanning = true`
- ✅ Input por polling directo (`Keyboard.current` + `Gamepad.current`)
- ✅ Detección de colisiones por `OverlapBox` (suelo y pared)
- ✅ Physics Material `NoFriction` en colliders
- ✅ Eventos: `OnGroundedChanged`, `OnJumped`, `OnDashed`, `OnWallJumped`, `OnWallSliding`

### 3.2 RoverStatsSO — Valores actuales del asset

> ⚠️ **Los valores del `.asset` difieren de los defaults del `.cs`.** El asset (`RoverStats_Default.asset`) tiene prioridad — es lo que Unity usa en runtime.

| Grupo | Parámetro | Valor en `.asset` | Default en `.cs` | GDD |
|-------|-----------|-------------------|-------------------|-----|
| **Run** | `maxRunSpeed` | 7.5 | 12 | 7.5 |
| | `groundAcceleration` | 40 | 40 | 25 (GDD) → tuneable |
| | `groundDeceleration` | 63 | 63 | 40 (GDD) → tuneable |
| | `airControlFactor` | 0.65 | 0.65 | 0.65 |
| **Jump** | `jumpForce` | **13.5** | 22 | 16 |
| | `jumpCutMultiplier` | **0.3** | 0.5 | 0.5 |
| | `coyoteTime` | **0.08** | 0.12 | 0.12 |
| | `jumpBufferTime` | 0.1 | 0.1 | 0.10 |
| **Fall** | `maxFallSpeed` | **−18** | −26 | −26 |
| **Dash** | `dashSpeed` | 28 | 28 | 28 |
| | `dashDuration` | 0.2 | 0.2 | 0.18 |
| | `dashCooldown` | 1.2 | 1.2 | 1.2 |
| | `dashSleepTime` | 0.028 | 0.028 | 0.028 |
| **Wall** | `wallSlideSpeed` | −2 | −2 | — |
| | `wallJumpForceX` | 14 | 14 | — |
| | `wallJumpForceY` | 18 | 18 | — |
| | `wallJumpInputLock` | 0.15 | 0.15 | 0.15 |
| **Physics** | `gravityScale` | 20 | 20 | 20 |
| | `fallGravityMultiplier` | **2.8** | 1.4 | 1.8 |

> Los valores en **negrita** son los que más difieren del GDD. Fueron ajustados durante playtesting.

### 3.3 PlayerAnimatorController — Bridge desacoplado

> **Documento detallado:** [AnimatorSetup.md](file:///c:/Users/LENOVO/RedDustLegacy/Docs/Architecture/AnimatorSetup.md)

- ✅ `Speed` (Float) → controla transición Idle ↔ Walk (normalizado `|velocityX| / maxRunSpeed`)
- ✅ `IsGrounded` (Bool) → suscrito a `OnGroundedChanged`
- ✅ `IsJumping` (Bool) → `!IsGrounded`
- ✅ `IsScanning` (Bool) → `controller.IsScanning`
- ✅ Flip del sprite basado en input directo (`_sprite.flipX`)
- ⚠️ Contiene `Debug.Log` / `Debug.LogError` temporales que deben limpiarse

### 3.4 PlayerAudioController — SFX del rover

- ✅ **Crossfade dual-source:** transición suave entre sfxIdle ↔ sfxWalk usando dos AudioSources
- ✅ **Fade out en aire:** detiene motor al despegar del suelo (fade 0.15 s)
- ✅ **One-shots:** jump, damage, death via `oneShotAudioSource.PlayOneShot()`
- ✅ **Integración AudioManager:** `PlayDeathSound()` llama a `AudioManager.Instance.TriggerGameOverMusic()`
- ⚠️ **Bug:** `OnDashed` está suscrito a `PlayDamageSound` — debería ser un SFX de dash o removerse

### 3.5 AudioManager — Música global

- ✅ Singleton `DontDestroyOnLoad`
- ✅ `PlayMusic(clip, loop)` para BGM
- ✅ `TriggerGameOverMusic()` — sin loop
- ✅ `PlayGlobalSFX(clip)` via `PlayOneShot`
- ℹ️ Namespace: `Core` — requiere `using Core;` para acceder

### 3.6 SceneLoader — Navegación

- ✅ Singleton `DontDestroyOnLoad`
- ✅ `LoadMainMenu()`, `LoadLevel01()`, `LoadLevel02()`
- ✅ `LoadNextLevel()` — avanza por buildIndex, vuelve a menú si no hay más
- ✅ `ReloadCurrentScene()`

---

## 4. Animator Controller — RoverAC

**Archivos:**
```
Assets/Animations/Rover/
├── Rover_Idle.anim    (25 frames, 12 fps, loop ☑)
├── Rover_walk.anim    (25 frames, 16 fps, loop ☑)
├── Rover_Run.anim     (pendiente de integración en transiciones)
├── Rover_Jump.anim    (25 frames, 20 fps, loop ☐)
├── Rover_Scan.anim    (pendiente de integración en transiciones)
└── RoverAC.controller
```

**Parámetros del Animator:**

| Parámetro | Tipo | Uso |
|-----------|------|-----|
| `Speed` | Float | Controla Idle ↔ Walk |
| `IsGrounded` | Bool | Estado en suelo |
| `IsJumping` | Bool | Controla entrada/salida de Jump |
| `IsScanning` | Bool | Estado de escaneo |

**Transiciones configuradas:**

| Origen | Destino | Condición | Has Exit Time | Duration |
|--------|---------|-----------|:---:|----------|
| Idle | Walk | `Speed > 0.1` | ☐ | 0 |
| Walk | Idle | `Speed < 0.1` | ☐ | 0 |
| Idle | Jump | `IsJumping = true` | ☐ | 0 |
| Walk | Jump | `IsJumping = true` | ☐ | 0 |
| Jump | Idle | `IsJumping = false` | ☐ | 0.05 |

> **Write Defaults:** desactivado en todos los nodos.

---

## 5. Controles configurados

**Archivo:** `Assets/Input/RoverInputActions_Local.inputactions`

| Acción | Tipo | Bindings teclado | Notas |
|--------|------|-----------------|-------|
| Move | Value (Vector2) | `←→` / `A D` (WASD completo) | Solo se usa eje X en `ReadInput()` |
| Jump | Button | `W` / `↑` | GDD especifica `Espacio` — discrepancia |
| Dash | Button | `Left Shift` | ✅ Alineado con GDD |
| Scan | Button | `E` | GDD especifica `F` — discrepancia |

> ⚠️ **Discrepancias con GDD §2:** Jump debería ser `Espacio`, Scan debería ser `F`. Los bindings actuales (`W`/`↑` para Jump, `E` para Scan) son funcionales pero no coinciden con la documentación de diseño.

---

## 6. Configuración de Physics 2D

```
Edit → Project Settings → Physics 2D
├── Simulation Mode:     Fixed Update
└── Layer Collision Matrix:
      Player  ↔ Ground  ☑
      Default ↔ Ground  ☑

Assets/Settings/Physics/NoFriction.physicsMaterial2D
├── Friction:    0
└── Bounciness:  0
```

---

## 7. Git — Estructura de ramas

```
main        ← producción
develop     ← integración
  ├── feature/player-movement
  ├── feature/ui-hud
  ├── feature/level-design
  ├── feature/audio-system
  ├── feature/enemy-ai
  └── feature/vfx-shaders
```

---

## 8. Pendientes para Sprint 2

| Prioridad | Tarea | Ref. GDD |
|-----------|-------|----------|
| 🔴 Alta | Tilemap del Level01 con plataformas y paredes | §9.2 |
| 🔴 Alta | Sistema de Integridad Estructural (SI) + `DegradationSystem` | §4 |
| 🔴 Alta | Prefab del Player guardado en repositorio | — |
| 🟡 Media | Integrar `Rover_Run.anim`, `Rover_Jump.anim`, `Rover_Scan.anim` en RoverAC | §13.3 |
| 🟡 Media | Corregir bindings: Jump → `Espacio`, Scan → `F` | §2 |
| 🟡 Media | Primer enemigo con IA (Drone Patrullero) | §8.4 |
| 🟡 Media | HUD mínimo: barra de SI + slots de celda | §10 |
| 🟡 Media | Probar wall jump en escena real con geometría Tilemap | §3.4 |
| 🟢 Baja | Limpiar `Debug.Log` de `PlayerAnimatorController` | — |
| 🟢 Baja | Corregir suscripción `OnDashed → PlayDamageSound` en `PlayerAudioController` | — |
| 🟢 Baja | Level02 con contenido | §9.3 |
| 🟢 Baja | Sistema de checkpoints | §7 |