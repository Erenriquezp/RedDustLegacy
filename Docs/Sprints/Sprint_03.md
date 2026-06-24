# Sprint 03 — Sistemas core, HUD, segundo enemigo y checkpoints

> **Estado:** ⬜ Pendiente (nada iniciado) · **Revisado:** 2026-06-23 · **Prerequisito:** Sprint 02 · **Ref. GDD:** §4, §7, §8.4, §10, §15

Integridad Estructural (SI), HUD, Drone Patrullero con proyectiles, checkpoints con respawn y GameManager con pausa/game-over. **Confirmado al 2026-06-22:** ninguno de estos scripts existe todavía (`DegradationSystem`, `HUDManager`, `GameManager`, `DronePatrollerAI`, `EnemyProjectile`, `Checkpoint`) y `PlayerController` aún no expone `OnDamageReceived` / `OnDeath` / `OnLanded` / `SetStats`.

**Dependencias de assets ya resueltas (adelanto):**
- ✅ Sprites del Drone listos: `Drone-idle-v1`, `Drone-walk-v1`, `Drone-attack-v1`, `Drone-drone_dead-v1` (`Art/Sprites/`) → desbloquea el Animator `DronePatrollerAC` de T3.
- ✅ `PlayerAudioController` ya tiene `PlayDeathSound()`, `sfxDeath` y `sfxDamage` cableados → listos para enganchar a `OnDeath` / `OnDamageReceived` (T1) y al Game Over (T5).

## Equipo y ramas

| Rol | Rama | Tareas |
|-----|------|--------|
| Gameplay Programmer (Líder) | `feature/player-movement` | T1 (SI), apoyo T4 |
| Technical Director | `feature/ui-hud` / `develop` | T2 (HUD), T5 (GameManager) |
| AI Programmer | `feature/enemy-ai` | T3 (Drone) |
| Level Designer | `feature/level-design` | T4 (checkpoints, colocación) |

## Dependencias y orden

Orden recomendado: **T1 → T2 → T5 → T4 → T3**.

```
T1 DegradationSystem ──┬─→ T2 HUDManager      (OnSIChanged / OnPhaseChanged / OnDeath)
                       ├─→ T3 DronePatroller  (proyectil → TakeDamage)
                       ├─→ T4 Checkpoints     (lee/escribe SI)
                       └─→ T5 GameManager     (OnDeath)
T4 CheckpointManager ──→ T5 GameManager       (RespawnPlayer)
T2 Alert Strip ────────→ T4 Checkpoints       ("CHECKPOINT REGISTRADO")
```

---

## T1 — Sistema de Integridad Estructural (SI)
**Responsable:** Gameplay Programmer · **Rama:** `feature/player-movement` · **Ref. GDD:** §4 · **Arq.:** [PlayerController.md](../Architecture/PlayerController.md)

Núcleo del que dependen las demás tareas. Crear `Scripts/Player/DegradationSystem.cs` (en el GameObject del Player, junto a `PlayerController`).

### Cambios previos en `PlayerController.cs`
Añadir (el resto del controlador no se toca):
```csharp
public event Action<float> OnDamageReceived;  // cantidad de daño
public event Action OnDeath;                    // SI = 0
public event Action OnLanded;                   // disparar en CheckCollisions, transición a grounded
public void SetStats(RoverStatsSO stats) => _stats = stats;
```
`OnLanded` se invoca en el bloque `!wasGrounded && _isGrounded` de `CheckCollisions()`, junto al `OnGroundedChanged?.Invoke(true)` existente.

### API de `DegradationSystem`
```csharp
public float CurrentSI { get; }
public int   CurrentPhase { get; }      // 1–6
public int   CellsInReserve { get; }     // 0–2
public void  TakeDamage(float amount);
public void  UseSolarCell();             // +20 SI, cap 100, consume reserva
public void  SetSI(float value);         // usado por el respawn (T4)
public event Action<float> OnSIChanged;
public event Action<int>   OnPhaseChanged;
public event Action        OnDeath;
```

### Valores base (GDD §4.1)
SI máx 100 · inicio 74 · muerte `< 1` · celda `+20 SI` (máx 2 en reserva) · los checkpoints **no** restauran SI.

### Fases (acumulativas) — implementar **1–4** este sprint; 5–6 a Sprint 04
| Fase | SI | Modificadores |
|------|----|---------------|
| 1 NOMINAL | 100–74% | sin cambios |
| 2 DESGASTE | 73–61% | daño por caída desde 2 u (no 3 u) |
| 3 AVERÍA | 60–47% | `dashCooldown ×1.5`, sin dash aéreo |
| 4 CRÍTICO | 46–29% | `maxRunSpeed ×0.75`, `groundAcceleration ×0.80`, `jumpBufferTime = 0.06` |
| 5 EMERGENCIA | 28–9% | `jumpForce ×0.85`, sin wall-jump, `coyoteTime = 0.06` |
| 6 EXTINCIÓN | 8–1% | `maxRunSpeed ×0.50`, sin dash |

### Aplicación de modificadores
1. En `Awake()`: guardar una copia base de los stats y crear una copia runtime — `_runtime = Instantiate(_controller.GetStats());` → `_controller.SetStats(_runtime);`.
2. Al cruzar un umbral: recalcular fase, copiar valores base sobre `_runtime` y reaplicar los modificadores acumulados hasta esa fase; emitir `OnPhaseChanged`.
3. Nunca editar el `.asset` original (la copia runtime evita corromper el tuning compartido).

### Fuentes de daño (GDD §4.3)
| Fuente | SI | Implementación |
|--------|----|----------------|
| Contacto Biol | 5 SI/s | el `OnTriggerStay2D` del Biol llama `TakeDamage(contactDamage * Time.fixedDeltaTime)` |
| Proyectil drone | 8–12 | `EnemyProjectile` (T3) llama `TakeDamage` al impactar |
| Caída 3–5 u | 10 | trackear `velocityY` entre `OnLanded`s |
| Caída ≥6 u | 25 (cap) | mismo cálculo, cap a 25 |

Daño por caída: registrar la `velocityY` máxima de caída durante el aire y, en `OnLanded`, estimar la distancia; umbral mínimo 2 u en Fase ≥2, 3 u en Fase 1.

**DoD:** la SI baja con daño de enemigos y caídas; las fases 1–4 aplican sus modificadores (verificar que en Fase 4 el rover corre más lento); la celda restaura +20 SI; SI=0 dispara `OnDeath` una sola vez.

---

## T2 — HUD mínimo
**Responsable:** Technical Director · **Rama:** `feature/ui-hud` · **Ref. GDD:** §10 · **Dep.:** T1

Crear `Scripts/UI/HUDManager.cs` en el Canvas de Level01 (Screen Space Overlay, referencia 1920×1080, Scale With Screen Size). Solo presenta datos; sin lógica de juego.

### Suscripciones (resolver la referencia a `DegradationSystem` por escena)
```csharp
void OnEnable() {
    _degradation.OnSIChanged    += UpdateSIBar;
    _degradation.OnPhaseChanged += UpdatePhaseEffects;
    _degradation.OnDeath        += ShowGameOver;   // o delegar en GameManager (T5)
}
// simétrico en OnDisable
```

### Elementos MVP
| Elemento | Componentes | Comportamiento |
|----------|-------------|----------------|
| Barra de SI | `Slider` + `Image` fill + `TMP_Text` | valor numérico visible; pulso `scale 1.0→1.08→1.0` en 0.12 s al recibir daño |
| Slots de celda (×2) | 2 `Image` | lleno = icono brillante; vacío = gris 40% |
| Alert Strip | `TMP_Text` + fondo semitransparente | mayúsculas monoespacio; fade 0.5 s al ocultar |

### Color de la barra (GDD §10.2)
100–61% `#4CAF50` · 60–41% `#FFA726` · 40–21% `#F44336` (texto parpadea) · 20–1% `#8B0000` (parpadeo rápido).

### Método público para el Alert Strip
`public void ShowAlert(string text, float duration)` — lo usará T4 para `CHECKPOINT REGISTRADO`.

**DoD:** la barra refleja la SI en tiempo real y cambia de color por umbral; los slots muestran lleno/vacío; pulso visual al recibir daño; el Alert Strip aparece y se desvanece correctamente.

---

## T3 — Drone Patrullero (2º enemigo)
**Responsable:** AI Programmer · **Rama:** `feature/enemy-ai` · **Ref. GDD:** §8.4 · **Dep.:** T1

Scripts: `Scripts/AI/DronePatrollerAI.cs`, `Scripts/AI/EnemyProjectile.cs`; asset `DronePatrollerStats.asset` (reutilizar `EnemyStatsSO`, ampliándolo si hace falta con `attackRange`, `projectileSpeed`, `projectileLifetime`, `attackCooldown`).

### FSM (4 estados)
```
Patrol (waypoints, moveSpeedBase 3.0)
  ├─ rover < 7 u ─────────────→ Chase (moveSpeed 5.0)
  │                                ├─ rover < 3.5 u → Attack (dispara c/1.5 s)
  │                                └─ rover fuera > 5 s → Return (→ waypoint más cercano → Patrol)
```
Waypoints: `Transform[]` asignado en escena; en Patrol, avanzar al siguiente al llegar (`MoveTowards`). Flip por `transform.localScale.x` según dirección, como en el Biol.

### `EnemyProjectile.cs`
- Prefab `Prefabs/Enemies/DroneProjectile.prefab`, layer `EnemyProjectile`: `Rigidbody2D` Kinematic (gravity 0), `CircleCollider2D` radio 0.2 trigger, `SpriteRenderer`.
- `Start()` → `Destroy(gameObject, lifetime)` (2 s). `Update()` → mover recto `direction * speed * Time.deltaTime` (8 u/s).
- `OnTriggerEnter2D`: si `Player` → `GetComponentInParent<DegradationSystem>()?.TakeDamage(Random.Range(8,13))` y destruir; si capa Ground → destruir.

### Animator `DronePatrollerAC`
Sprites ya completos: `Drone-idle-v1`, `Drone-walk-v1`, `Drone-attack-v1`, `Drone-drone_dead-v1` (en `Art/Sprites/`). Falta crear el `.controller` y los clips. Params `Speed` (Float), `IsAttacking` (Bool), `IsDead` (Bool).

### Disparo
`firerPoint` (Transform hijo) como origen; en Attack, instanciar el proyectil hacia el rover cada `attackCooldown` (1.5 s).

**DoD:** patrulla entre waypoints; persigue al detectar (<7 u); dispara c/1.5 s en attackRange (<3.5 u); proyectiles a 8 u/s viven 2 s y causan 8–12 SI; vuelve al waypoint si el rover escapa >5 s; HP=0 → Death → desactivar.

---

## T4 — Checkpoints
**Responsable:** Level Designer + Gameplay Programmer · **Rama:** `feature/level-design` · **Ref. GDD:** §7 · **Dep.:** T1, T5

Scripts: `Scripts/Level/Checkpoint.cs` (trigger individual) y `Scripts/Level/CheckpointManager.cs` (singleton de escena).

### `Checkpoint.cs`
- `BoxCollider2D` trigger (ancho ≥3 u, alto 4 u). Campo `checkpointID` y flag `_activated` (un solo registro).
- `OnTriggerEnter2D(Player)` → si no activado: `CheckpointManager.Instance.Register(transform.position, DegradationSystem.CurrentSI)`, efecto visual (pulso de luz 1.2 s) y `HUDManager.ShowAlert("CHECKPOINT REGISTRADO", 2f)`.

### `CheckpointManager.cs`
- Guarda `respawnPosition` y `savedSI` del último checkpoint.
- `public void RespawnPlayer(PlayerController player, DegradationSystem deg)`: mueve al Player a `respawnPosition` y llama `deg.SetSI(savedSI)` (la SI **no** vuelve a 100). Enemigos derrotados y celdas recogidas **no** reaparecen.

### Colocación en Level01 (Level Designer)
3 checkpoints: inicio (Z1), post-Z2 y pre-Leviatán (Z4-Z5). Cada uno ≥8 u del enemigo más cercano.

**DoD:** cruzar un checkpoint muestra el aviso en el Alert Strip; al morir, el rover reaparece en el último con la SI guardada y el estado del nivel no se resetea.

---

## T5 — GameManager
**Responsable:** Technical Director · **Rama:** `feature/player-movement` / `develop` · **Ref. GDD:** §15.1 · **Dep.:** T1, T4

Crear `Scripts/Core/GameManager.cs`, singleton `DontDestroyOnLoad`. Máquina de estados global; coordina muerte → Game Over → respawn. Punto de contacto entre `DegradationSystem`, `CheckpointManager`, `HUDManager` y `AudioManager`.

### Estados
`enum GameState { MainMenu, Playing, Paused, GameOver, Cinematic }`

| Estado | Entrada | Acción | `timeScale` |
|--------|---------|--------|:-:|
| Playing | carga Level01 / reanudar | input activo | 1 |
| Paused | `Esc` | mostrar `PausePanel` | 0 |
| GameOver | `DegradationSystem.OnDeath` | mostrar `GameOverPanel` + `AudioManager.TriggerGameOverMusic()` | 0 |
| Cinematic | trigger de cinemática | congelar `PlayerController` | 1 |

### UI en el Canvas de Level01 (desactivados por defecto)
- `PausePanel`: Reanudar (`ResumeGame`), Reintentar (`RestartFromCheckpoint`), Menú (`ReturnToMainMenu`).
- `GameOverPanel` ("SISTEMA INOPERATIVO"): Reintentar, Menú.
- `RestartFromCheckpoint()` → `CheckpointManager.Instance.RespawnPlayer(...)` y vuelve a `Playing` (timeScale 1).

### Input de pausa
Añadir acción `Pause` en `RoverInputActions_Local.inputactions` (`Escape` / gamepad `Start`, no reasignable) → callback `GameManager.Instance.TogglePause()`.

**DoD:** `Esc` pausa (`timeScale 0`) y muestra el menú con las 3 opciones; SI=0 muestra Game Over; Reintentar respawna en el checkpoint sin resetear el nivel; Menú carga MainMenu correctamente.

---

## Entregables

| Tarea | Responsable | Entregable | Dep. |
|-------|-------------|------------|------|
| T1 — SI | Gameplay Programmer | `DegradationSystem.cs` + eventos en `PlayerController`, fases 1–4 | Sprint 02 |
| T2 — HUD | Technical Director | Barra SI + slots + Alert Strip | T1 |
| T3 — Drone | AI Programmer | FSM 4 estados + `EnemyProjectile` con daño | T1 |
| T4 — Checkpoints | Level Designer + Gameplay | 3 checkpoints + respawn | T1, T5 |
| T5 — GameManager | Technical Director | Pausa + Game Over + respawn | T1, T4 |
