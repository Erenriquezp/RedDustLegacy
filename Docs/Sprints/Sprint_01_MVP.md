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

### Pendiente para el MVP

- ⬜ Cinemachine — cámara que sigue al jugador
- ⬜ Tilemap — nivel de prueba real
- ⬜ Estados de animación: Jump, Fall, Dash, WallSlide
- ⬜ Sistema de Integridad Estructural (SI) del GDD
- ⬜ Escena de prueba con plataformas y paredes para validar wall jump

---