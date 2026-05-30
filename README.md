## Red Dust Legacy

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
