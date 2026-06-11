# Sprint 03 — Sistemas core, HUD, segundo enemigo y checkpoints

> **Objetivo:** Sistema de Integridad Estructural funcional, HUD mínimo, Drone Patrullero con proyectiles, checkpoints con respawn, y GameManager con pausa/game over.  
> **Estado:** ⬜ Pendiente  
> **Prerequisito:** [Sprint 02](file:///c:/Users/LENOVO/RedDustLegacy/Docs/Sprints/Sprint_02.md) completado (Level01 con geometría, Ser Bioluminiscente, animaciones rover)  
> **Ref. GDD:** §4 SI, §7 Checkpoints, §8.4 Drone Patrullero, §10 HUD, §15 Arquitectura

---

## Tarea 1 — Sistema de Integridad Estructural (SI) y Degradación

**Rama git:** `feature/player-movement`  
**Responsable:** Gameplay Programmer  
**Ref. GDD:** §4 Sistema de SI, §4.2 Tabla de degradación, §4.3 Tabla de daño  
**Ref. Arquitectura:** [PlayerController.md](file:///c:/Users/LENOVO/RedDustLegacy/Docs/Architecture/PlayerController.md) §8 Extensión futura

### 1.1 DegradationSystem.cs (nuevo)

**Guardar en:** `Assets/Scripts/Player/DegradationSystem.cs`

```
Responsabilidad:
- Gestiona el valor de SI del rover (0–100)
- Determina la fase actual (1–6) según los rangos del GDD
- Aplica modificadores de fase al PlayerController
- Expone SI y fase al HUDManager en tiempo real
- Gestiona celdas de energía (recuperación de SI)

API pública:
- float CurrentSI { get; }
- int CurrentPhase { get; }         // 1–6
- void TakeDamage(float amount)
- void UseSolarCell()               // +20 SI, máx 2 en reserva
- int CellsInReserve { get; }       // 0, 1 o 2
- event Action<float> OnSIChanged   // notifica al HUD
- event Action<int> OnPhaseChanged  // notifica cambio de fase
- event Action OnDeath              // SI = 0
```

### 1.2 Parámetros base (GDD §4.1)

| Parámetro | Valor |
|-----------|-------|
| SI máxima | 100 |
| SI al inicio del juego | 74 |
| SI mínima funcional | 1 (debajo = muerte) |
| Checkpoints restauran SI | **No** — daño acumulado permanente |
| Celdas de energía | +20 SI por celda, máx 2 en reserva |
| Fuentes de daño | Ataques de enemigos, caídas >3 u, objetos ambientales |

### 1.3 Fases de degradación — Modificadores al PlayerController

> Los modificadores se aplican sobre los valores del ScriptableObject base. Los efectos son **acumulativos**: entrar en Fase 4 significa que los efectos de Fases 1–3 siguen activos.

| Fase | SI | Modificadores al movimiento | Efectos HUD/Audio |
|------|----|-----------------------------|-------------------|
| **1 — NOMINAL** | 100–74% | Sin cambios | Barra verde. Música exploración |
| **2 — DESGASTE** | 73–61% | Daño por caída activo desde 2 u (en vez de 3 u) | Barra verde-ámbar. Antena tiembla c/15 s |
| **3 — AVERÍA** | 60–47% | `dashCooldown × 1.5`, sin dashes aéreos, delay 0.04 s en dash | Barra ámbar. Sonido rueda raspando c/8-10 s |
| **4 — CRÍTICO** | 46–29% | `maxRunSpeed × 0.75`, `runAcceleration × 0.80`, `jumpBufferTime = 0.06` | Barra roja parpadeante. Tinte naranja. Alarma c/30 s |
| **5 — EMERGENCIA** | 28–9% | `jumpForce × 0.85`, wall jump desactivado, `coyoteTime = 0.06` | Tinte naranja intenso. Interferencia c/5 s |
| **6 — EXTINCIÓN** | 8–1% | `maxRunSpeed × 0.50`, sin dash, movimiento asimétrico | Estática en bordes. Minimapa falla. Apagones c/20 s |

> ⚠️ **Scope Sprint 03:** Implementar Fases 1–4 como mínimo. Fases 5–6 pueden postergarse a Sprint 04.

### 1.4 Implementación de modificadores

**Opción recomendada:** Copia runtime del ScriptableObject.

```csharp
// En DegradationSystem.Awake():
_runtimeStats = Instantiate(_controller.GetStats());  // copia runtime
_controller.SetStats(_runtimeStats);                   // nuevo método en PlayerController

// Al cambiar de fase:
void ApplyPhaseModifiers(int phase)
{
    // Resetear a valores base
    var baseStats = _baseStatsCopy;
    // Aplicar modificadores acumulativos según fase
    if (phase >= 3) _runtimeStats.dashCooldown = baseStats.dashCooldown * 1.5f;
    if (phase >= 4) _runtimeStats.maxRunSpeed = baseStats.maxRunSpeed * 0.75f;
    // etc.
}
```

> Esto requiere agregar `public void SetStats(RoverStatsSO stats)` al `PlayerController`.

### 1.5 Fuentes de daño — Implementación

| Fuente | SI perdida | Implementación técnica |
|--------|-----------|----------------------|
| Contacto con Ser Bioluminiscente | 5 SI/s | `OnTriggerStay2D` → `DegradationSystem.TakeDamage(5 * Time.fixedDeltaTime)` |
| Proyectil de Drone Patrullero | 8–12 SI | `OnTriggerEnter2D` en `EnemyProjectile.cs` → `TakeDamage(Random.Range(8,13))` |
| Caída 3–5 u | 10 SI | En `PlayerController`: registrar `velocityY` al despegar, calcular distancia al aterrizar |
| Caída ≥6 u | 25 SI (cap) | Mismo cálculo, cap máximo de 25 |
| Objeto ambiental (trampa) | 6 SI | `OnTriggerEnter2D` — solo Nivel 2 (Sprint futuro) |

### 1.6 Daño por caída — Lógica

```csharp
// En PlayerController o DegradationSystem:
private float _velocityYAtTakeoff;

void OnGroundedChanged(bool grounded)
{
    if (!grounded)
        _velocityYAtTakeoff = 0f;  // empezar a trackear
    
    if (grounded && _lastFrameVelocityY < -threshold)
    {
        float fallDistance = EstimateFallDistance(_lastFrameVelocityY);
        float minFallDamage = (currentPhase >= 2) ? 2f : 3f;  // Fase 2+: desde 2 u
        
        if (fallDistance >= 6f)       TakeDamage(25f);  // cap
        else if (fallDistance >= minFallDamage) TakeDamage(10f);
    }
}
```

### 1.7 Eventos nuevos en PlayerController

Agregar a [PlayerController.cs](file:///c:/Users/LENOVO/RedDustLegacy/Assets/Scripts/Player/PlayerController.cs):

```csharp
// Nuevos eventos
public event Action<float> OnDamageReceived;  // float = cantidad de daño
public event Action OnDeath;                   // SI = 0
public event Action OnLanded;                  // al aterrizar (para SFX + VFX)

// Nuevo método para DegradationSystem
public void SetStats(RoverStatsSO stats) { _stats = stats; }
```

**Definition of Done:** La SI disminuye al recibir daño de enemigos y por caída. Las fases 1–4 aplican los modificadores correctos al movimiento (verificar que el rover se mueve más lento en Fase 4). Las celdas de energía restauran +20 SI. SI = 0 dispara el evento `OnDeath`.

---

## Tarea 2 — HUD mínimo funcional

**Rama git:** `feature/ui-hud`  
**Responsable:** Technical Director  
**Ref. GDD:** §10 HUD e interfaz de usuario, §10.2 Rangos de color  
**Dependencia:** T1 (DegradationSystem) debe exponer `OnSIChanged` y `OnPhaseChanged`

### 2.1 HUDManager.cs (nuevo)

**Guardar en:** `Assets/Scripts/UI/HUDManager.cs`

```
Responsabilidad:
- Actualiza todos los elementos del Canvas en tiempo real
- Suscrito a eventos del DegradationSystem
- No contiene lógica de juego — solo presenta datos

Patrón: MonoBehaviour en el Canvas de Level01
```

### 2.2 Elementos MVP

| Elemento | Posición | Componente Unity | Comportamiento |
|----------|---------|-----------------|---------------|
| **Barra de SI** | Top-left `(32, −32)` | `Slider` + `Image` (fill) + `TextMeshPro` | Valor numérico siempre visible. Pulso al recibir daño: scale 1.0→1.08→1.0 en 0.12 s |
| **Slots de celda** (×2) | Top-left, debajo de barra SI | `Image` × 2 (sprite lleno/vacío) | Lleno: icono brillante. Vacío: gris 40% opacidad |
| **Alert Strip** | Bottom-center | `TextMeshPro` + fondo semitransparente | Solo con alerta activa. Mayúsculas monoespacio. Fade 0.5 s al terminar |

### 2.3 Rangos de color del HUD (GDD §10.2)

| Rango SI | Estado | Color barra (hex) | Color texto | Efectos adicionales |
|----------|--------|-------------------|-------------|-------------------|
| 100–61% | NOMINAL | `#4CAF50` verde | Blanco | Ninguno |
| 60–41% | ADVERTENCIA | `#FFA726` ámbar | Ámbar | Antena vibra en Idle |
| 40–21% | CRÍTICO | `#F44336` rojo | Rojo parpadeante | Tinte naranja en HUD. Alarma audio c/30 s |
| 20–1% | EXTINCIÓN | `#8B0000` rojo oscuro | Rojo oscuro parpadeante rápido | Tinte naranja intenso. Estática. Apagones |

### 2.4 Suscripciones

```csharp
void OnEnable()
{
    _degradation.OnSIChanged    += UpdateSIBar;
    _degradation.OnPhaseChanged += UpdatePhaseEffects;
    _degradation.OnDeath        += ShowGameOverScreen;
}
```

### 2.5 Hierarchy del Canvas

```
Level01
└── Canvas (Screen Space - Overlay, ref: 1920×1080, Scale With Screen Size)
      ├── SI_Group (Top-left)
      │     ├── SI_Bar (Slider)
      │     │     ├── Background (Image, negro 60% opacity)
      │     │     └── Fill (Image, color dinámico)
      │     ├── SI_Text (TextMeshPro, monoespacio)
      │     └── CellSlots
      │           ├── Cell_01 (Image)
      │           └── Cell_02 (Image)
      ├── AlertStrip (Bottom-center)
      │     ├── AlertBG (Image, negro 50% opacity)
      │     └── AlertText (TextMeshPro)
      └── HUDManager (script)
```

**Definition of Done:** La barra de SI refleja el valor actual en tiempo real. El color cambia al cruzar umbrales (verde → ámbar → rojo → rojo oscuro). Los slots de celda muestran lleno/vacío. Al recibir daño, la barra hace pulso visual (scale 1.08). El Alert Strip muestra `CHECKPOINT REGISTRADO` al cruzar un checkpoint.

---

## Tarea 3 — Segundo enemigo: Drone Patrullero

**Rama git:** `feature/enemy-ai`  
**Responsable:** AI Programmer  
**Ref. GDD:** §8.4 Drone Patrullero  
**Dependencia:** T1 (DegradationSystem) para que los proyectiles causen daño real

### 3.1 Scripts a crear

| Script | Ruta | Función |
|--------|------|---------|
| `DronePatrollerAI.cs` | `Scripts/AI/` | FSM de 4 estados + waypoints + disparo |
| `EnemyProjectile.cs` | `Scripts/AI/` | Proyectil recto con timer de vida y daño |
| `DronePatrollerStats.asset` | `Scripts/ScriptableObjects/` | Instancia SO con valores del GDD |

> Si `EnemyStatsSO.cs` no existe aún (debería crearse en Sprint 02 con el Biol), crearlo aquí como SO base.

### 3.2 Valores del GDD (§8.4)

| Parámetro | Valor |
|-----------|-------|
| HP | 60 |
| `moveSpeed` patrullaje | 3,0 u/s |
| `moveSpeed` Chase | 5,0 u/s |
| Rango detección | 7 u radio |
| `attackRange` | 3,5 u |
| Daño proyectil | 8–12 SI (variación aleatoria ±4) |
| Velocidad proyectil | 8,0 u/s — recto, sin seguimiento |
| Tiempo vida proyectil | 2,0 s |
| Hitbox proyectil | Círculo radio 0,2 u |
| Cooldown ataque | 1,5 s |
| Retorno a patrulla | Si rover escapa del rango por 5 s |

### 3.3 FSM — 4 estados

```
Patrol (waypoints fijos, 3.0 u/s)
  │
  ├─ rover en rango 7 u ──────────→ Chase (5.0 u/s)
  │                                    │
  │                                    ├─ rover en attackRange 3.5 u → Attack
  │                                    │     │                          │
  │                                    │     │  cooldown 1.5 s          │
  │                                    │     └──────────────────────────┘
  │                                    │
  │                                    └─ rover fuera de rango 5 s ──→ Return
  │                                                                      │
  └──────────────────────────────────────────────────────────────────────┘
                                (regresa al waypoint más cercano)
```

### 3.4 Animator Controller

**Guardar en:** `Assets/Animations/Enemies/DronePatrullero/DronePatrollerAC.controller`

Sprites existentes: `Drone-idle-v1.png`, `Drone-walk-v1.png`

| Estado | Sprite | FPS | Loop | Condición |
|--------|--------|-----|:----:|-----------|
| `Patrol` | `Drone-idle-v1.png` | 8 | ☑ | Estado base |
| `Chase` | `Drone-walk-v1.png` | 12 | ☑ | Rover detectado |
| `Attack` | ⬜ Por crear | 12 | ☐ | `attackRange` alcanzado |
| `Death` | ⬜ Por crear | 10 | ☐ | HP = 0 |

**Parámetros:** `Speed` (Float), `IsAttacking` (Bool), `IsDead` (Bool)

### 3.5 Proyectil enemigo

**Prefab:** `Assets/Prefabs/Enemies/DroneProjectile.prefab`  
**Layer:** `EnemyProjectile` (colisiona con Player y Ground, GDD §16.1)

```
Componentes del prefab:
├── Rigidbody2D
│     ├── Body Type:     Kinematic
│     └── Gravity Scale: 0
├── CircleCollider2D
│     ├── Radius:        0.2
│     └── Is Trigger:    true
├── SpriteRenderer
│     └── (sprite del proyectil)
└── EnemyProjectile.cs
      ├── speed:     8.0 u/s
      ├── lifetime:  2.0 s
      └── damage:    Random.Range(8, 13)
```

```csharp
// EnemyProjectile.cs — lógica core
void Start() => Destroy(gameObject, lifetime);

void Update() => transform.Translate(direction * speed * Time.deltaTime);

void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        other.GetComponent<DegradationSystem>()?.TakeDamage(damage);
        Destroy(gameObject);
    }
    else if (groundLayer.Contains(other.gameObject.layer))
    {
        Destroy(gameObject);
    }
}
```

### 3.6 Prefab del Drone Patrullero

**Guardar en:** `Assets/Prefabs/Enemies/DronePatrullero.prefab`

```
Componentes:
├── Rigidbody2D (Dynamic o Kinematic según diseño)
├── BoxCollider2D (hitbox del enemigo)
├── DronePatrollerAI
│     ├── Stats: DronePatrollerStats
│     ├── Waypoints: Transform[] (asignar en escena)
│     ├── Projectile Prefab: DroneProjectile.prefab
│     └── Fire Point: Transform (hijo para punto de disparo)
├── Animator → DronePatrollerAC
└── SpriteRenderer
```

**Definition of Done:** El Drone patrulla entre waypoints. Al detectar al rover (<7 u), lo persigue. Al estar en attackRange (<3.5 u), dispara cada 1.5 s. Los proyectiles viajan recto a 8 u/s, viven 2 s y causan 8–12 SI de daño. Si el rover escapa >5 s, el Drone regresa al waypoint. Al llegar HP = 0, reproduce animación Death y se desactiva.

---

## Tarea 4 — Sistema de checkpoints

**Rama git:** `feature/level-design`  
**Responsable:** Level Designer + Gameplay Programmer  
**Ref. GDD:** §7 Sistema de checkpoints  
**Dependencia:** T1 (DegradationSystem) para registrar la SI al activar, T5 (GameManager) para gestionar respawn

### 4.1 Scripts a crear

| Script | Ruta | Función |
|--------|------|---------|
| `Checkpoint.cs` | `Scripts/Level/` | Trigger individual. Registra posición y SI al activar |
| `CheckpointManager.cs` | `Scripts/Level/` | Singleton de escena. Gestiona checkpoint activo y lógica de respawn |

### 4.2 Checkpoint.cs — Especificación

```csharp
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointID;
    private bool _activated = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_activated && other.CompareTag("Player"))
        {
            _activated = true;
            float currentSI = DegradationSystem.Instance.CurrentSI;
            CheckpointManager.Instance.RegisterCheckpoint(this, currentSI);
            // Efecto visual: pulso de luz 1.2 s
            // HUD: Alert Strip → "CHECKPOINT REGISTRADO"
        }
    }
}
```

### 4.3 Parámetros del GDD (§7)

| Parámetro | Valor |
|-----------|-------|
| Activación | Automática al cruzar `BoxCollider2D` (trigger). Sin input |
| Dimensiones trigger | Ancho: ancho del pasillo (mín 3 u). Alto: 4 u |
| Radio mínimo de seguridad | ≥8 u del enemigo más cercano |
| Efecto visual | Pulso de luz 1.2 s |
| Texto HUD | `CHECKPOINT REGISTRADO` vía Alert Strip |
| SI al revivir | **No se restaura** — misma SI que al activar el checkpoint |
| Distribución Niv.1 | 3: inicio, post Zona 2, pre-Leviatán |
| Distribución Niv.2 | 4: entrada, Ala A, Ala B, pre-Centinela |

### 4.4 CheckpointManager.cs — Lógica de respawn

```csharp
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    
    private Checkpoint _activeCheckpoint;
    private float _savedSI;
    private Vector3 _respawnPosition;

    public void RegisterCheckpoint(Checkpoint cp, float currentSI)
    {
        _activeCheckpoint = cp;
        _savedSI = currentSI;
        _respawnPosition = cp.transform.position;
    }

    public void RespawnPlayer(PlayerController player, DegradationSystem degradation)
    {
        player.transform.position = _respawnPosition;
        degradation.SetSI(_savedSI);  // restaurar SI del checkpoint, no 100
        // Enemigos derrotados: NO reaparecen
        // Celdas recogidas: NO reaparecen
    }
}
```

### 4.5 Secuencia de muerte y respawn

```
1. SI = 0 → DegradationSystem.OnDeath
2. GameManager recibe OnDeath → estado = GameOver
3. PlayerController: reproducir animación Death (12 frames / 8 FPS ≈ 1.5 s)
4. Mostrar pantalla Game Over con opción "Reintentar"
5. Al presionar Reintentar:
   a. Teletransportar Player a _respawnPosition
   b. Restaurar SI al valor guardado (savedSI, NO a 100)
   c. Los enemigos derrotados NO reaparecen
   d. Las celdas recogidas NO reaparecen
   e. GameManager → estado = Playing
```

**Definition of Done:** Al cruzar un trigger de checkpoint, el texto `CHECKPOINT REGISTRADO` aparece en el Alert Strip. Al morir (SI = 0), el rover reaparece en el último checkpoint con la SI correcta. Los enemigos derrotados y celdas recogidas no reaparecen tras el respawn. Los 3 checkpoints del Level01 están posicionados según el beat map.

---

## Tarea 5 — GameManager

**Rama git:** `feature/player-movement` o `develop`  
**Responsable:** Technical Director  
**Ref. GDD:** §15.1 Módulos principales  
**Dependencia:** T1 (DegradationSystem.OnDeath), T4 (CheckpointManager.RespawnPlayer)

### 5.1 Script a crear

**Guardar en:** `Assets/Scripts/Core/GameManager.cs`

```
Responsabilidad:
- Máquina de estados global de la partida
- Congela/descongela el juego (Time.timeScale)
- Coordina muerte → Game Over → respawn
- Punto de contacto entre DegradationSystem, CheckpointManager y HUDManager

Patrón: Singleton + DontDestroyOnLoad
```

### 5.2 Estados de la partida

```csharp
public enum GameState { MainMenu, Playing, Paused, GameOver, Cinematic }
```

| Estado | Transición desde | Acción | `Time.timeScale` |
|--------|-----------------|--------|:-:|
| `Playing` | Carga de Level01 / Reanudar | Habilitar input, reanudar física | 1 |
| `Paused` | `Esc` durante Playing | Mostrar menú pausa, deshabilitar input de juego | 0 |
| `GameOver` | `DegradationSystem.OnDeath` | Mostrar pantalla, opciones: Reintentar / Menú principal | 0 |
| `Cinematic` | Trigger de flashback / secuencia final | Congelar PlayerController, reproducir cinemática | 1 (gestionado por CinematicManager) |

### 5.3 Menú de pausa — Hierarchy

```
Level01
└── Canvas
      └── PausePanel (desactivado por defecto)
            ├── PauseBG (Image, negro 70% opacity, full screen)
            ├── PauseTitle (TextMeshPro: "PAUSA")
            ├── ResumeButton → GameManager.ResumeGame()
            ├── RestartButton → GameManager.RestartFromCheckpoint()
            └── QuitButton → GameManager.ReturnToMainMenu()
```

### 5.4 Pantalla de Game Over — Hierarchy

```
Level01
└── Canvas
      └── GameOverPanel (desactivado por defecto)
            ├── GameOverBG (Image, negro 85% opacity, full screen)
            ├── GameOverTitle (TextMeshPro: "SISTEMA INOPERATIVO")
            ├── RetryButton → GameManager.RestartFromCheckpoint()
            └── MenuButton → GameManager.ReturnToMainMenu()
```

### 5.5 Flujo del GameManager

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    void OnEnable()
    {
        // Suscribirse a muerte
        DegradationSystem.Instance.OnDeath += HandlePlayerDeath;
    }

    // Esc → pausa
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing) PauseGame();
        else if (CurrentState == GameState.Paused) ResumeGame();
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    public void HandlePlayerDeath()
    {
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        AudioManager.Instance.TriggerGameOverMusic();
    }

    public void RestartFromCheckpoint()
    {
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        CheckpointManager.Instance.RespawnPlayer(player, degradation);
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadMainMenu();
    }
}
```

### 5.6 Input de pausa

Agregar acción `Pause` en `RoverInputActions_Local.inputactions`:
- Binding: `Escape` (teclado), `Start` (gamepad)
- **No reasignable** (GDD §2)
- Callback → `GameManager.Instance.TogglePause()`

**Definition of Done:** Presionar `Esc` pausa el juego (`Time.timeScale = 0`) y muestra menú con Reanudar / Reintentar / Menú principal. Al morir (SI = 0), aparece pantalla "SISTEMA INOPERATIVO" con opciones Reintentar / Menú principal. Reintentar respawna en el checkpoint sin resetear el estado del nivel. Volver al menú carga la escena MainMenu correctamente.

---

## Dependencias entre tareas

```
T1 — DegradationSystem
  ↓ expone OnSIChanged, OnPhaseChanged, OnDeath, TakeDamage()
  ├──→ T2 — HUDManager (suscrito a eventos de SI)
  ├──→ T3 — DronePatrullero (proyectiles llaman TakeDamage)
  ├──→ T4 — Checkpoints (registran SI actual)
  └──→ T5 — GameManager (suscrito a OnDeath)

T4 — CheckpointManager
  ↓ expone RespawnPlayer()
  └──→ T5 — GameManager (llama RespawnPlayer al reintentar)

T2 — HUDManager
  ↓ expone Alert Strip
  └──→ T4 — Checkpoints (muestra "CHECKPOINT REGISTRADO")
```

**Orden de implementación recomendado:**
1. **T1** — DegradationSystem (todas las demás tareas dependen de él)
2. **T2** — HUDManager (para poder visualizar SI durante desarrollo)
3. **T5** — GameManager (para tener pausa y game over mientras se prueban enemigos)
4. **T4** — Checkpoints (requiere DegradationSystem + GameManager)
5. **T3** — Drone Patrullero (requiere DegradationSystem para daño real)

---

## Resumen de entregables

| Tarea | Responsable | Entregable clave | Dependencia |
|-------|------------|------------------|-------------|
| T1 — Sistema SI | Gameplay Programmer | `DegradationSystem.cs` con fases 1–4 funcionales | Sprint 02 completado |
| T2 — HUD mínimo | Technical Director | Barra SI + slots celda + Alert Strip | T1 |
| T3 — Drone Patrullero | AI Programmer | Prefab con FSM 4 estados + proyectil con daño | T1 |
| T4 — Checkpoints | Level Designer + Gameplay | 3 checkpoints en Level01 + respawn funcional | T1, T5 |
| T5 — GameManager | Technical Director | Pausa + Game Over + respawn desde checkpoint | T1, T4 |
