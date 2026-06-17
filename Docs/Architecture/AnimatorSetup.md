# AnimatorSetup — Documentación de Arquitectura

> **Archivos:**  
> [PlayerAnimatorController.cs](file:///c:/Users/LENOVO/RedDustLegacy/Assets/Scripts/Player/PlayerAnimatorController.cs)  
> [RoverAC.controller](file:///c:/Users/LENOVO/RedDustLegacy/Assets/Animations/Rover/RoverAC.controller)  
> **Ref. GDD:** §13.3 Animator Controller del Rover

---

## 1. Visión general

El sistema de animación del rover usa un patrón **bridge desacoplado**: `PlayerAnimatorController` actúa como intermediario entre `PlayerController` (lógica de movimiento) y el `Animator` de Unity (máquina de estados visual). Ningún script de lógica escribe directamente al Animator.

```
PlayerController          PlayerAnimatorController          Animator (RoverAC)
     │                            │                              │
     ├─ GetMoveInput() ──────────→├─ SetFloat("Speed") ─────────→│
     ├─ IsGrounded ──────────────→├─ SetBool("IsJumping") ──────→│
     ├─ IsScanning ──────────────→├─ SetBool("IsScanning") ─────→│
     ├─ OnGroundedChanged ───────→├─ SetBool("IsGrounded") ─────→│
     └─ (no sabe del Animator)    └─ flipX del SpriteRenderer    │
```

---

## 2. PlayerAnimatorController.cs

### 2.1 Componentes obtenidos en Awake

| Componente | Variable | Búsqueda | Notas |
|------------|----------|----------|-------|
| `Animator` | `_animator` | `GetComponentInChildren<Animator>()` | Debe estar en el hijo `Sprite` |
| `PlayerController` | `_controller` | `GetComponent<PlayerController>()` | En el mismo GameObject |
| `Rigidbody2D` | `_rb` | `GetComponent<Rigidbody2D>()` | En el mismo GameObject (padre) |
| `SpriteRenderer` | `_sprite` | `GetComponentInChildren<SpriteRenderer>()` | En el hijo `Sprite` |
| `RoverStatsSO` | `_stats` | `_controller.GetStats()` | Para normalizar velocidad |

### 2.2 Parámetros del Animator (hashed)

Los nombres de parámetros se convierten a hash en tiempo de compilación para evitar comparaciones de string en cada frame:

```csharp
private static readonly int _speedHash      = Animator.StringToHash("Speed");
private static readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
private static readonly int _isJumpingHash  = Animator.StringToHash("IsJumping");
private static readonly int _isScanningHash = Animator.StringToHash("IsScanning");
```

### 2.3 Update — Lógica por frame

```csharp
void Update()
{
    // Guard clause: si no hay Animator o Controller asignado, no hacer nada
    if (_animator == null || _animator.runtimeAnimatorController == null) return;

    // 1. Speed normalizado (0–1) para transición Idle ↔ Walk
    float normalizedSpeed = Abs(rb.linearVelocity.x) / stats.maxRunSpeed;
    _animator.SetFloat("Speed", normalizedSpeed);

    // 2. Flip del sprite basado en input directo (no en velocidad)
    if (inputX > 0.01f)  sprite.flipX = false;
    if (inputX < -0.01f) sprite.flipX = true;

    // 3. Jump: true cuando no está en suelo
    _animator.SetBool("IsJumping", !controller.IsGrounded);

    // 4. Scan: true mientras se mantiene el botón
    _animator.SetBool("IsScanning", controller.IsScanning);
}
```

### 2.4 Suscripción a eventos

| Evento | Acción |
|--------|--------|
| `OnGroundedChanged(bool)` | `SetBool("IsGrounded", isGrounded)` |

Se suscribe en `OnEnable()` y desuscribe en `OnDisable()`.

### 2.5 Notas de implementación

- **Flip por input, no por velocidad:** El sprite se voltea basado en `GetMoveInput()`, no en `rb.linearVelocity.x`. Esto hace que el flip sea instantáneo al presionar la dirección, no después de que la aceleración mueva al rover.
- **Speed normalizado:** Se usa `|velocityX| / maxRunSpeed` para que el parámetro sea independiente de los valores absolutos del SO. Siempre entre 0 y ~1.
- **Guard clause:** Si el `runtimeAnimatorController` es null (ej: durante setup), el script no crashea.

---

## 3. Animator Controller — RoverAC

### 3.1 Archivos de animación

| Archivo | Frames | FPS | Loop | Estado actual |
|---------|--------|-----|:----:|---------------|
| `Rover_Idle.anim` | 25 | 12 | ☑ | ✅ Integrado en RoverAC |
| `Rover_walk.anim` | 25 | 16 | ☑ | ✅ Integrado en RoverAC |
| `Rover_Jump.anim` | 25 | 20 | ☐ | ✅ Integrado en RoverAC |
| `Rover_Run.anim` | — | — | — | ⬜ No integrado en transiciones |
| `Rover_Scan.anim` | — | — | — | ⬜ No integrado en transiciones |

### 3.2 Parámetros del Animator

| Parámetro | Tipo | Escrito por | Uso |
|-----------|------|-------------|-----|
| `Speed` | Float | `Update()` cada frame | Controla transición Idle ↔ Walk |
| `IsGrounded` | Bool | `HandleGroundedChanged()` | Indica contacto con suelo |
| `IsJumping` | Bool | `Update()` cada frame | `= !IsGrounded`. Controla entrada/salida de Jump |
| `IsScanning` | Bool | `Update()` cada frame | Controla entrada a Scan |

### 3.3 Estados y transiciones

```
                    ┌──────────┐
                    │   Idle   │◄─── Estado por defecto
                    └────┬─────┘
                         │
            Speed > 0.1  │  Speed < 0.1
                         ▼
                    ┌──────────┐
                    │   Walk   │
                    └────┬─────┘
                         │
          IsJumping=true │  IsJumping=true
          (desde Idle    │   (desde Walk
           también)      ▼    también)
                    ┌──────────┐
                    │   Jump   │
                    └────┬─────┘
                         │
        IsJumping=false  │
                         ▼
                    ┌──────────┐
                    │   Idle   │
                    └──────────┘
```

| # | Origen | Destino | Condición | Has Exit Time | Transition Duration |
|---|--------|---------|-----------|:---:|-----|
| 1 | Idle | Walk | `Speed > 0.1` | ☐ | 0 s |
| 2 | Walk | Idle | `Speed < 0.1` | ☐ | 0 s |
| 3 | Idle | Jump | `IsJumping = true` | ☐ | 0 s |
| 4 | Walk | Jump | `IsJumping = true` | ☐ | 0 s |
| 5 | Jump | Idle | `IsJumping = false` | ☐ | 0.05 s |

**Configuración de todos los nodos:**
- `Write Defaults` = **OFF** en todos los estados
- Transition interruption = None (default)

### 3.4 Configuración del Animator en el Inspector

```
Player (GameObject)
└── Sprite (child)
      ├── SpriteRenderer
      │     └── Material: Sprites-Default (o Lit)
      └── Animator
            ├── Controller:        RoverAC
            ├── Apply Root Motion:  ☐
            ├── Update Mode:        Normal
            └── Culling Mode:       Always Animate
```

---

## 4. Comparación con GDD — Estados MVP pendientes

El GDD (§13.3) define 9 estados MVP y 9 estados V2. Estado actual de implementación:

| Estado GDD | Frames | FPS | Condición | Implementado |
|------------|--------|-----|-----------|:---:|
| `Idle` | 8 | 8 | Sin input | ✅ (como Rover_Idle, 25f/12fps) |
| `Run` | 10 | 12 | `velocidadX > 0.1` | ⬜ (existe archivo, no integrado) |
| `Jump_Rise` | 6 | 12 | `!isGrounded && velocidadY > 0` | ⚠️ (Jump existe, pero no distingue Rise/Fall) |
| `Jump_Fall` | 4 | 8 | `!isGrounded && velocidadY ≤ 0` | ⬜ |
| `Land` | 4 | 12 | Transición desde Jump_Fall | ⬜ |
| `Dash` | 6 | 24 | `isDashing` | ⬜ |
| `Damage` | 5 | 12 | `onDamageReceived()` | ⬜ |
| `Death` | 12 | 8 | `SI = 0` | ⬜ |
| `Scan_Loop` | 8 | 8 | `isScanning` | ⚠️ (existe archivo Rover_Scan, no integrado) |

> ℹ️ La implementación actual usa `Walk` donde el GDD especifica `Run`. Los frames/FPS actuales (25f/12-16fps) difieren de los del GDD (8-10f/8-12fps). Esto es aceptable durante el prototipo — los assets finales del Artist reemplazarán los placeholders.

---

## 5. Extensión futura

### 5.1 Estados pendientes para Sprint 2

Para completar el MVP del Animator según el GDD, se necesitan estos parámetros y transiciones adicionales:

**Nuevos parámetros:**

| Parámetro | Tipo | Uso |
|-----------|------|-----|
| `IsDashing` | Bool | → estado Dash |
| `VelocityY` | Float | → distinguir Jump_Rise vs Jump_Fall |
| `IsDamaged` | Trigger | → estado Damage (one-shot) |
| `IsDead` | Bool | → estado Death |

**Nuevos eventos a suscribir:**

| Evento | Acción en Animator |
|--------|-------------------|
| `OnDashed` | `SetBool("IsDashing", true)` + resetear cuando termina |
| `OnDamageReceived` | `SetTrigger("IsDamaged")` |
| `OnDeath` | `SetBool("IsDead", true)` |

### 5.2 Separación Jump_Rise / Jump_Fall

El GDD requiere animaciones separadas para subida y caída. Para implementarlo:

```csharp
// En Update() de PlayerAnimatorController:
_animator.SetFloat("VelocityY", _rb.linearVelocity.y);
```

Y en el Animator Controller:
- Estado `Jump_Rise`: condición `IsJumping = true && VelocityY > 0`
- Estado `Jump_Fall`: condición `IsJumping = true && VelocityY ≤ 0`
- Transición `Jump_Rise → Jump_Fall`: `VelocityY ≤ 0`

### 5.3 Estado de degradación visual (V2)

Para las animaciones de degradación (`Idle_Degrade`, `Run_Limp` del GDD):

```csharp
// Nuevo parámetro:
private static readonly int _degradePhaseHash = Animator.StringToHash("DegradePhase");

// En Update():
_animator.SetInteger("DegradePhase", degradationSystem.CurrentPhase);
```

Esto permite usar sub-state machines en el Animator Controller donde cada fase de degradación tiene su variante de Idle/Run.

### 5.4 Consideraciones de rendimiento

- Los hashes de parámetros ya están optimizados (static readonly)
- `SetFloat/SetBool` en cada `Update()` es negligible (~4 calls × 73 bytes = ~292 bytes/frame)
- Si se agregan muchos estados, considerar usar **Animator Override Controllers** para las fases de degradación en vez de sub-state machines

---

## 6. Checklist de setup en Unity

Para configurar el sistema de animación en una nueva escena o al rehacer el prefab:

- [ ] GameObject `Player` tiene `PlayerController` y `PlayerAnimatorController`
- [ ] Child `Sprite` tiene `SpriteRenderer` + `Animator`
- [ ] `Animator.Controller` = `RoverAC`
- [ ] `Animator.Apply Root Motion` = OFF
- [ ] `Animator.Culling Mode` = Always Animate
- [ ] RoverAC tiene parámetros: `Speed` (Float), `IsGrounded` (Bool), `IsJumping` (Bool), `IsScanning` (Bool)
- [ ] Todos los estados en RoverAC tienen `Write Defaults` = OFF
- [ ] Todas las transiciones tienen `Has Exit Time` = OFF (excepto Jump → Idle: 0.05)
- [ ] Todas las transiciones tienen `Transition Duration` = 0 (excepto Jump → Idle: 0.05)
