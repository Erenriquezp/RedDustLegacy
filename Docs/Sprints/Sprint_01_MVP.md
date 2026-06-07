## Resumen del Prototipo — Red Dust Legacy MVP

### Arquitectura del Proyecto

```
Assets/
├── Animations/Rover/
│   ├── RoverAC.controller      ← Animator Controller
│   ├── Rover_Idle.anim         ← 25 frames, 12fps, loop
│   └── Rover_walk.anim         ← 25 frames, 16fps, loop
├── Art/Sprites/                ← Spritesheets cortados
├── Input/
│   └── RoverInputActions       ← New Input System
├── ScriptableObjects/
│   └── RoverStats_Default      ← Todos los parámetros del GDD
└── Scripts/Player/
    ├── PlayerController.cs
    ├── PlayerAnimatorController.cs
    └── Data/RoverStatsSO.cs
```

---

### Hierarchy de la Escena

```
SampleScene
├── Main Camera
├── Global Light 2D
├── Ground                      ← BoxCollider2D, Layer: Ground
└── Player
      ├── Sprite                ← Animator (RoverAC) + SpriteRenderer
      ├── GroundCheckPoint      ← posición Y: -0.6
      └── WallCheckPoint        ← posición X: ±0.5
```

---

### Componentes en Player

```
Rigidbody2D
  ├── Gravity Scale:       0    ← manual
  ├── Collision Detection: Continuous
  └── Interpolate:         Interpolate

CapsuleCollider2D

PlayerController
  ├── Stats:               RoverStats_Default
  ├── Ground Check Point:  GroundCheckPoint
  ├── Ground Layer:        Ground
  ├── Wall Check Point:    WallCheckPoint
  └── Wall Layer:          Ground

PlayerInput
  ├── Actions:             RoverInputActions
  ├── Default Map:         Player
  ├── Behavior:            Invoke Unity Events
  └── Events:
        ├── Jump → PlayerController.OnJumpInput
        └── Dash → PlayerController.OnDashInput

PlayerAnimatorController
  └── Animator Override:   Sprite
```

---

### Sistemas Implementados

**RoverStatsSO** — ScriptableObject con todos los parámetros tuneables desde el Inspector sin tocar código:
```
Run:   maxRunSpeed=7.5, acceleration=40, deceleration=63, airControl=0.65
Jump:  force=22, cutMultiplier=0.5, coyoteTime=0.12, bufferTime=0.1
Fall:  maxFallSpeed=-26, fallGravityMultiplier=1.8
Dash:  speed=28, duration=0.2, cooldown=1.2, sleepTime=0.028
Wall:  slideSpeed=-2, jumpForceX=14, jumpForceY=18, inputLock=0.15
```

**PlayerController** — movimiento completo:
- ✅ Carrera con aceleración/desaceleración diferenciada suelo/aire
- ✅ Gravedad manual con caída más pesada que subida
- ✅ Salto variable (jump cut al soltar el botón)
- ✅ Coyote time (0.12s)
- ✅ Jump buffer (0.1s)
- ✅ Dash con freeze frames y gravity=0
- ✅ Un dash aéreo por salto, se restaura al aterrizar
- ✅ Wall slide con velocidad limitada
- ✅ Wall jump con lock de input horizontal
- ✅ Input por polling directo (sin oscilaciones de callback)

**PlayerAnimatorController** — bridge desacoplado:
- ✅ Idle → Walk al presionar movimiento
- ✅ Walk → Idle al soltar
- ✅ Flip del sprite según dirección
- ✅ Suscrito a `OnGroundedChanged` para futuras transiciones
- ✅ Lee input desde `GetMoveInput()` del PlayerController

**Animator Controller RoverAC:**
- ✅ 2 estados: Rover_Idle, Rover_walk
- ✅ Transiciones sin Exit Time, Duration=0
- ✅ Write Defaults desactivado en ambos nodos
- ✅ Parámetros: Speed (Float), IsGrounded (Bool)

---

### Controles Configurados

| Tecla | Acción |
|---|---|
| A / D o ←/→ | Moverse |
| Space | Saltar |
| Left Shift | Dash |

---
## Resumen completo del proyecto — Red Dust Legacy

### Escenas

```
Assets/Scenes/
  ├── MainMenu.unity   ← pantalla de inicio funcional
  ├── Level01.unity    ← escena de juego con el rover
  └── Level02.unity    ← creada, pendiente de contenido
```

---

### MainMenu

**Hierarchy:**
```
MainMenu
  ├── Main Camera         (Orthographic, Z: -10)
  ├── StarParticles       (Particle System 3D, Z: 10)
  │     ├── Material:     ParticlesUnlit
  │     ├── Shape:        Box 15x1x8
  │     └── Color:        #AFA9EC
  ├── Canvas              (Screen Space - Camera)
  │     ├── Background    (Image #0A0A1A, full screen)
  │     ├── CenterGroup   (Vertical Layout Group)
  │     │     ├── Subtitle  "A GAME BY RED DUST TEAM"
  │     │     ├── Title     "RED DUST LEGACY"
  │     │     ├── Divider   (línea horizontal)
  │     │     ├── Tagline   "Survive. Explore. Adapt."
  │     │     ├── PlayButton  → SceneLoader.LoadLevel01()
  │     │     └── QuitButton  → Application.Quit()
  └── SceneLoader         (DontDestroyOnLoad)
```

**Scripts UI:**
```
Assets/Scripts/UI/
  └── MainMenuController.cs   OnPlayPressed / OnQuitPressed

Assets/Scripts/Core/
  └── SceneLoader.cs          Singleton, DontDestroyOnLoad
                              LoadMainMenu / LoadLevel01 / LoadLevel02
                              LoadNextLevel / ReloadCurrentScene
```

---

### Level01 — Sistema de juego

**Hierarchy:**
```
Level01
  ├── Main Camera
  ├── Global Light 2D
  ├── CinemachineCamera       (sigue al Player)
  ├── Ground                  (BoxCollider2D, Layer: Ground, Material: NoFriction)
  └── Player
        ├── Sprite            (Animator → RoverAC, SpriteRenderer)
        ├── WallCheckPoint
        └── GroundCheckPoint
```

**Componentes del Player:**
```
Rigidbody2D
  ├── Gravity Scale:        0  (manual)
  ├── Collision Detection:  Continuous
  ├── Interpolate:          None
  └── Freeze Rotation Z:    ☑

CapsuleCollider2D
  ├── Size:      X:1.8  Y:0.8
  ├── Direction: Vertical
  └── Material:  NoFriction

PlayerController
PlayerInput         (RoverInputActions_Local)
PlayerAnimatorController
```

---

### Scripts del Player

```
Assets/Scripts/Player/
  ├── PlayerController.cs
  ├── PlayerAnimatorController.cs
  └── Data/RoverStatsSO.cs

Assets/ScriptableObjects/
  └── RoverStats_Default.asset
```

**PlayerController — mecánicas implementadas:**
```
✅ Movimiento horizontal con aceleración/desaceleración
✅ Gravedad manual con caída pesada
✅ Salto variable (jump cut al soltar)
✅ Coyote time (0.12s)
✅ Jump buffer (0.1s)
✅ Dash con freeze frames
✅ Un dash aéreo por salto
✅ Wall slide con velocidad limitada
✅ Wall jump con lock de input
✅ Input por polling directo (Keyboard.current)
✅ Physics Material NoFriction en colliders
```

**RoverStats_Default — valores:**
```
Run:   maxRunSpeed=7.5, acceleration=40, deceleration=63, airControl=0.65
Jump:  force=22, cutMultiplier=0.5, coyoteTime=0.12, bufferTime=0.1
Fall:  maxFallSpeed=-26, fallGravityMultiplier=1.8
Dash:  speed=28, duration=0.2, cooldown=1.2, sleepTime=0.028
Wall:  slideSpeed=-2, jumpForceX=14, jumpForceY=18, inputLock=0.15
```

---

### Animator Controller — RoverAC

```
Assets/Animations/Rover/
  ├── Rover_Idle.anim    (25 frames, 12fps, loop ☑)
  ├── Rover_Walk.anim    (25 frames, 16fps, loop ☑)
  ├── Rover_Jump.anim    (25 frames, 20fps, loop ☐)
  └── RoverAC.controller
```

**Estados y transiciones:**
```
Parámetros:
  Speed      Float  → controla Idle ↔ Walk
  IsGrounded Bool   → estado en suelo
  IsJumping  Bool   → controla entrada/salida de Jump

Transiciones:
  Idle  →  Walk    Speed > 0.1,   Has Exit Time ☐, Duration 0
  Walk  →  Idle    Speed < 0.1,   Has Exit Time ☐, Duration 0
  Idle  →  Jump    IsJumping true, Has Exit Time ☐, Duration 0
  Walk  →  Jump    IsJumping true, Has Exit Time ☐, Duration 0
  Jump  →  Idle    IsJumping false, Has Exit Time ☐, Duration 0.05
```

---

### Controles configurados

```
Assets/Input/RoverInputActions_Local.inputactions

  Move:   Flechas ←→  y  A/D
  Jump:   W  y  Flecha ↑
  Dash:   Left Shift
```

---

### Git — Estructura de ramas

```
main      ← producción (solo tú sin approvals)
develop   ← integración
  ├── feature/player-movement
  ├── feature/ui-hud
  ├── feature/level-design
  ├── feature/audio-system
  ├── feature/enemy-ai
  └── feature/vfx-shaders
```

---

### Configuración de Physics 2D

```
Simulation Mode:  Fixed Update
Layer Matrix:     Player ↔ Ground  ☑
                  Default ↔ Ground ☑

Assets/Settings/NoFriction.physicsMaterial2D
  Friction:    0
  Bounciness:  0
```

---

### Pendiente para completar el Sprint 1

```
⬜ Tilemap del Level01 con plataformas y paredes
⬜ Prefab del Player guardado en repo
⬜ Probar wall jump en escena real
⬜ Commit completo al repo
⬜ Level02 con contenido
⬜ Sistema de Integridad Estructural (GDD §2.6)
```
---