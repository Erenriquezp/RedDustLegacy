# Especificación Técnica — Opportunity: Red Dust Legacy
**Versión:** 1.0 | **Motor:** Unity 6.4 (6000.4.8f1) | **Pipeline:** URP

---

## Stack tecnológico — obligatorio para todos los miembros

- Unity 6.4 (6000.4.8f1) — no actualizar sin aprobación del Technical Director
- Universal Render Pipeline (URP) — no cambiar a Built-in ni HDRP
- New Input System (Input System Package) — no usar el Input System legacy
- TextMeshPro — para todo texto en UI
- Cinemachine — para cámara del jugador
- Git + Git LFS — flujo de ramas obligatorio (ver sección Git)

---

## Estructura de carpetas — no modificar sin consenso

```
Assets/
├── Animations/Rover/       → clips y Animator Controller del rover
├── Animations/Enemies/     → un subfolder por enemigo
├── Art/Backgrounds/        → capas de parallax
├── Art/Sprites/            → spritesheets PNG (sin cortar)
├── Audio/Music/            → pistas de música
├── Audio/SFX/              → efectos de sonido
├── Input/                  → RoverInputActions_Local.inputactions
├── Prefabs/Player/         → Player.prefab
├── Prefabs/Enemies/        → un prefab por tipo de enemigo
├── Scenes/                 → MainMenu, Level01, Level02
├── Scripts/AI/             → FSMs y AIManager
├── Scripts/Core/           → SceneLoader, AudioManager, GameManager
├── Scripts/Level/          → ParallaxController, CheckpointManager
├── Scripts/Player/         → PlayerController, PlayerAnimatorController
├── Scripts/UI/             → MainMenuController, HUDController
├── ScriptableObjects/      → RoverStats_Default, EnemyStats por tipo
└── Settings/               → NoFriction.physicsMaterial2D, RoverAC
```

---

## Capas (Layers) — configuradas en Project Settings

| Layer | Uso |
|---|---|
| Default | Objetos sin clasificar |
| Ground | Suelo y plataformas con colisión |
| Player | GameObject Player |
| Enemy | Todos los enemigos |
| PlayerProjectile | Proyectiles del rover |
| EnemyProjectile | Proyectiles de enemigos |
| Platform | Plataformas atravesables |
| Interactable | Objetos escaneables, coleccionables |
| Trigger | Zonas de evento sin colisión física |

La Layer Collision Matrix está configurada en `Project Settings → Physics 2D`. No modificarla sin consultar al Technical Director.

---

## Reglas de physics

- Simulation Mode: `Fixed Update`
- Physics Material del Player y Ground: `NoFriction` (Friction: 0, Bounciness: 0)
- Todo objeto con Rigidbody2D dinámico: `Collision Detection: Continuous`
- El Player tiene `Gravity Scale: 0` — la gravedad se maneja en `PlayerController.cs`

---

## Convención de sprites

- Formato: PNG con transparencia
- Tamaño de tile/celda: 32 × 32 px
- Pixels Per Unit: 32
- Filter Mode: Point (no filter)
- Compression: None
- Sprite Mode: Multiple (todos los spritesheets)
- Nomenclatura: `NombrePersonaje-estado-v#.png`
  - Ejemplo: `Opportunity-dash-v1.png`, `Drone-attack-v1.png`

---

## Tarea 1 — Fondos y Parallax (S)

**Rama git:** `feature/vfx-shaders`

**Entregables:**
- 3 imágenes PNG de fondo para Nivel 1 (cuevas de Meridiani Planum)
- 3 imágenes PNG de fondo para Nivel 2 (Relicto marciano)
- Tileset de cuevas exportado como spritesheet (mínimo: suelo, pared, techo, borde, esquinas)

**Especificación técnica de capas parallax (GDD §parallax):**

| Capa | Factor | Descripción visual |
|---|---|---|
| Fondo lejano | 0.15× | Formaciones rocosas distantes, niebla rojiza |
| Formaciones medias | 0.45× | Cristales, columnas de roca, flora marciana |
| Foreground | 0.85× | Rocas en primer plano, bordes de cueva |

**Paleta de color Nivel 1 (GDD §5.1):** rojos oxidados (#8B3A2A, #C4512E), azules profundos (#1A2A4A, #243560), bioluminiscencia (#1D9E75, #5DCAA5).

**Implementación en Unity:**
1. Crear GameObject `ParallaxBackground` en Level01
2. Añadir el script `ParallaxController.cs` (el Technical Director lo provee)
3. Asignar cada capa como hijo con su factor correspondiente

**Definition of Done:** Las 3 capas se desplazan a velocidades distintas visibles al mover el rover por el nivel. No hay bordes ni repeticiones visibles en una pantalla 1920×1080.

---

## Tarea 2 — Diseño de Nivel 1 (Level Designer)

**Rama git:** `feature/level-design`

**Entregables:**
- Beat map en papel o digital antes de implementar en Unity
- `Level01.unity` jugable de inicio a fin
- `Level02.unity` con estructura básica (sin contenido completo)

**Especificación del beat map (GDD §beat map):**
```
Zona 1 — Entrada (sala tutorial implícita)
  → Plataformas simples, sin enemigos, objeto escaneable SC-01
  → Enseña: salto, movimiento horizontal

Zona 2 — Primera tensión
  → 1–2 Seres Bioluminiscentes patrullando
  → Plataformas con gaps, requiere dash para cruzar uno
  → Checkpoint al final de la zona

Zona 3 — Respiro + recurso
  → Celda de energía accesible sin combate
  → Objeto escaneable SC-02
  → Acceso a zona inaccesible (requiere Rueda Reforzada — bloquear visualmente)

Zona 4 — Escalada
  → Combinación de Drone Patrullero + Biol
  → Plataformas verticales, wall jump requerido
  → Objeto escaneable SC-03

Zona 5 — Boss (pendiente implementación de boss)
  → Sala amplia, sin obstáculos en el suelo
  → Espacio para 3 fases de combate
```

**Restricciones técnicas:**
- Usar únicamente el tileset provisto por el Artist
- Layer del tilemap de colisión: `Ground`
- Layer del tilemap visual (sin colisión): `Default`
- El 30% del nivel debe ser inaccesible sin el upgrade Rueda Reforzada (marcar con bloques de color diferente durante desarrollo)
- Posicionar el prefab `Player.prefab` en el punto de inicio, no crear un Player nuevo

**Definition of Done:** El jugador puede completar el recorrido de Zona 1 a Zona 4 sin errores de consola. Las 3 zonas inaccesibles están visibles pero bloqueadas.

---

## Tarea 3 — Audio (K)

**Rama git:** `feature/audio-system` (o rama propia del Technical Director)

**Entregables:**
- `AudioManager.cs` con Singleton y DontDestroyOnLoad
- Unity Audio Mixer configurado
- SFX del rover importados y asignados

**Arquitectura del AudioManager (GDD §5.4):**

```csharp
// Estados de música
public enum MusicState { Silence, Exploration, Tension, Combat }

// Crossfade times (GDD §5.4.1)
// Exploration → Tension:  1.5s
// Tension → Combat:       0.5s
// Combat → Exploration:   3.0s
```

**Audio Mixer — 4 buses obligatorios:**
- Master (volumen global)
- Music (música adaptativa)
- SFX (efectos del rover y enemigos)
- Ambient (sonido ambiental de cueva)

**SFX del rover — prioridad de implementación (GDD §5.4.2):**

| Prioridad | Evento | Trigger en código |
|---|---|---|
| 1 | Ruedas rodando | Mientras `_rb.linearVelocity.x != 0` y `IsGrounded` |
| 1 | Aterrizaje | `OnGroundedChanged(true)` |
| 1 | Dash | `OnDashed` |
| 1 | Daño recibido | `OnDamageReceived` (pendiente implementar) |
| 2 | Escaneo | Al activar objeto escaneable |
| 2 | Transmisión | Al completar una zona |

**Definition of Done:** Al entrar a una sala con enemigos la música transiciona a Tension en 1.5s. Al eliminar todos los enemigos vuelve a Exploration. El aterrizaje del rover reproduce SFX audible.

---

## Tarea 4 — Animaciones de enemigos (KC)

**Rama git:** `feature/enemy-ai`

**Estados requeridos por enemigo (GDD §5.3.2):**

### Ser Bioluminiscente
Sprites existentes: `Biol-idle-v1.png`, `Biol-walk-v1.png`

| Estado | Frames | FPS | Loop | Prioridad |
|---|---|---|---|---|
| Idle | existente | 8 | Sí | MVP |
| Walk | existente | 10 | Sí | MVP |
| Attack | 6–8 | 12 | No | MVP |
| Hurt | 4 | 16 | No | MVP |
| Death | 8 | 10 | No | MVP |

### Drone Patrullero
Sprites existentes: `Drone-idle-v1.png`, `Drone-walk-v1.png`

| Estado | Frames | FPS | Loop | Prioridad |
|---|---|---|---|---|
| Idle | existente | 8 | Sí | MVP |
| Walk/Patrol | existente | 12 | Sí | MVP |
| Alert | 4 | 10 | No | MVP |
| Attack | 6 | 14 | No | MVP |
| Death | 8 | 12 | No | MVP |

### Drone Detector
Sprites existentes: `Drone Detector-idle-v1.png`

| Estado | Frames | FPS | Loop | Prioridad |
|---|---|---|---|---|
| Idle | existente | 8 | Sí | MVP |
| Scan | 10 | 10 | Sí | MVP |
| Alert | 4 | 12 | No | MVP |
| Chase | 8 | 14 | Sí | V2 |
| Attack | 6 | 14 | No | V2 |

### Centinela Secundario
Sprites existentes: `Centinela Secundario-idle.png`

| Estado | Frames | FPS | Loop | Prioridad |
|---|---|---|---|---|
| Idle | existente | 6 | Sí | MVP |
| Walk | 8 | 10 | Sí | MVP |
| Attack | 8 | 14 | No | MVP |
| Hurt | 4 | 16 | No | V2 |
| Death | 10 | 10 | No | V2 |

**Animator Controller por enemigo:**
- Parámetros mínimos: `Speed` (Float), `IsAttacking` (Bool), `IsDead` (Bool)
- Guardar en: `Assets/Animations/Enemies/NombreEnemigo/EnemigAC.controller`

**Definition of Done:** Cada enemigo reproduce Idle en la escena. Al activar `IsAttacking = true` en Play Mode el estado cambia correctamente.

---

## Tarea 5 — Sprites faltantes del rover (Artist — Rover)

**Rama git:** `feature/vfx-shaders` o rama del artista

**Sprites que faltan para MVP (bloqueantes):**

| Sprite | Frames | FPS | Notas |
|---|---|---|---|
| `Opportunity-dash.png` | 6–8 | 16 | Animación horizontal rápida, cuerpo inclinado |
| `Opportunity-wallslide.png` | 4–6 | 8 | Loop, rover apoyado en pared, deslizamiento lento |
| `Opportunity-hurt.png` | 4 | 16 | No loop, flash de daño |
| `Opportunity-land.png` | 4 | 16 | No loop, impacto de aterrizaje |
| `Opportunity-walljump.png` | 4 | 14 | No loop, impulso desde la pared |

**Sprites para V2 (no bloqueantes para MVP):**
- `Opportunity-phase2.png` hasta `phase4.png` — degradación visual progresiva
- `Opportunity-transmit.png` — animación de transmisión al JPL
- `Opportunity-upgrade-equip.png` — equipar upgrade

**Referencia de proporciones (GDD §5.2):**
- Mantener las proporciones del rover existente
- Los ojos (Pancam) deben ser visibles y expresivos en todos los estados
- Paleta consistente con los sprites existentes

**Definition of Done:** Los 5 sprites MVP están cortados en el Sprite Editor, con Loop Time configurado correctamente, y el Technical Director los ha integrado en el `RoverAC.controller`.

---

## Flujo de trabajo Git — obligatorio

```
1. Antes de trabajar:
   git checkout feature/tu-rama
   git fetch origin
   git rebase origin/develop

2. Commits con formato Conventional Commits:
   feat(enemies): add Biol attack animation
   art(sprites): add Opportunity-dash spritesheet
   fix(audio): correct crossfade timing

3. Al terminar una feature:
   git push origin feature/tu-rama
   Abrir Pull Request → base: develop
   Mínimo 1 approval antes de mergear

4. NUNCA hacer push directo a main o develop
5. SIEMPRE incluir los archivos .meta en los commits
6. NUNCA subir la carpeta Library/ o Temp/
```

---

## Reglas generales del proyecto

- No agregar assets de Asset Store sin aprobación del Technical Director
- No modificar `PlayerController.cs`, `RoverStatsSO.cs` ni `SceneLoader.cs` sin consultar al Gameplay Programmer
- No cambiar la resolución de referencia del Canvas (1920×1080)
- No cambiar la configuración de Physics 2D sin consultar al Technical Director
- Toda duda técnica se resuelve primero en el canal del equipo, no individualmente

---