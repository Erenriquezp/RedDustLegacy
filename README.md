## Red Dust Legacy
Metroidvania 2D — Unity 6.4

## Setup
1. Clona el repo
2. Abre en Unity 6.4 (6000.4.8f1)
3. Rama de desarrollo: `develop`

## Estructura de ramas
- `main` → producción
- `develop` → integración
- `feature/*` → desarrollo individual
```text
     ├── 👤 player-movement     (Miembro 1)
     ├── 👤 ui-hud              (Miembro 2)
     ├── 👤 level-design        (Miembro 3)
     ├── 👤 audio-system        (Miembro 4)
     ├── 👤 enemy-ai            (Miembro 5)
     └── 👤 vfx-shaders         (Miembro 6)
```

## Equipo
| Miembro | Rama |
|---|---|
| Líder | feature/player-movement |
| ... | feature/ui-hud |

## Reglas del Repositorio y Git

* ❌ **Nunca** hagas `push` directo a la rama `main`.
* ❌ **Nunca** hagas `push` directo a la rama `develop`.
* ❌ **Nunca** apruebes o hagas `merge` de tu propio *Pull Request (PR)* sin la revisión de otro miembro.
* ✅ **Siempre** haz `git rebase develop` en tu rama local antes de abrir un *PR* para evitar conflictos.
* ✅ **Siempre** incluye los archivos `.meta` de Unity en tus commits (¡no los ignores!).
* ✅ Abre **un PR por cada feature** terminada. No acumules semanas de trabajo en un solo commit o pull request gigante.
* ✅ Los conflictos de Git se resuelven **en tu propia rama**, nunca directamente sobre `develop`.

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
