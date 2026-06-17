# PlayerController — Documentación de Arquitectura

> **Archivo:** [PlayerController.cs](file:///c:/Users/LENOVO/RedDustLegacy/Assets/Scripts/Player/PlayerController.cs)  
> **Namespace:** Global  
> **RequireComponent:** `Rigidbody2D`, `CapsuleCollider2D`, `PlayerInput`  
> **Ref. GDD:** §2 Controles, §3 Mecánicas del jugador, §3.5 Tabla maestra de movimiento

---

## 1. Visión general

`PlayerController` es el script central del movimiento del rover. Implementa un sistema de física manual (`gravityScale = 0` en el Rigidbody2D) donde toda la velocidad se calcula frame a frame en `_frameVelocity` y se aplica al final de cada `FixedUpdate`.

**Patrón de ejecución:**

```
Update()                         FixedUpdate()
  ├── TickTimers()                 ├── CheckCollisions()
  └── ReadInput()                  ├── HandleJumpBuffer()
                                   ├── HandleWallSlide()
                                   ├── HandleDash()
                                   ├── HandleJump()
                                   ├── HandleRun()
                                   ├── ApplyGravity()
                                   └── ApplyVelocity()  → rb.linearVelocity = _frameVelocity
```

- **Input** se lee en `Update()` para máxima responsividad
- **Física** se ejecuta en `FixedUpdate()` para determinismo
- Los timers se decrementan con `Time.deltaTime` (no `fixedDeltaTime`)

---

## 2. Dependencias

### 2.1 SerializeField (Inspector)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `_stats` | `RoverStatsSO` | ScriptableObject con todos los parámetros tuneables |
| `_groundCheckPoint` | `Transform` | Posición del raycast de suelo (hijo `GroundCheckPoint`) |
| `_groundCheckSize` | `Vector2` | Tamaño del OverlapBox de suelo. Default: `(0.7, 0.05)` |
| `_groundLayer` | `LayerMask` | Capa contra la que se detecta suelo |
| `_wallCheckPoint` | `Transform` | Posición del raycast de pared (hijo `WallCheckPoint`) |
| `_wallCheckSize` | `Vector2` | Tamaño del OverlapBox de pared. Default: `(0.05, 0.8)` |
| `_wallLayer` | `LayerMask` | Capa contra la que se detectan paredes |

### 2.2 Componentes obtenidos en Awake

| Componente | Variable | Uso |
|------------|----------|-----|
| `Rigidbody2D` | `_rb` | Aplicar velocidad final |
| `PlayerInput` | `_playerInput` | Referencia al Input System (no usado directamente — input es por polling) |

### 2.3 Configuración del Rigidbody2D (Start)

```csharp
_rb.gravityScale = 0f;                              // gravedad manual
_rb.collisionDetectionMode = Continuous;             // evitar tunneling
_rb.interpolation = RigidbodyInterpolation2D.None;
_rb.constraints = FreezeRotation;
_rb.inertia = 1f;                                    // forzar inercia
_rb.WakeUp();                                        // despertar explícitamente
```

---

## 3. API pública

### 3.1 Propiedades de solo lectura

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `IsGrounded` | `bool` | ¿Está tocando el suelo? |
| `IsDashing` | `bool` | ¿Está en medio de un dash? |
| `FacingDir` | `int` | `1` = derecha, `−1` = izquierda |
| `IsScanning` | `bool` | ¿Está manteniendo el botón de escaneo? |

### 3.2 Métodos públicos

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `GetStats()` | `RoverStatsSO` | Acceso al ScriptableObject de stats |
| `GetMoveInput()` | `float` | Valor actual del input horizontal `[-1, 1]` |

### 3.3 Eventos (C# events)

| Evento | Tipo | Cuándo se dispara |
|--------|------|-------------------|
| `OnGroundedChanged` | `Action<bool>` | Al aterrizar (`true`) o despegar (`false`) |
| `OnJumped` | `Action` | Al ejecutar un salto |
| `OnDashed` | `Action` | Al iniciar un dash |
| `OnWallJumped` | `Action` | Al ejecutar un wall jump |
| `OnWallSliding` | `Action<bool>` | Al iniciar (`true`) o dejar (`false`) de deslizar en pared |

**Suscriptores actuales:**

| Evento | Suscriptor | Método |
|--------|------------|--------|
| `OnGroundedChanged` | `PlayerAnimatorController` | `HandleGroundedChanged(bool)` |
| `OnGroundedChanged` | `PlayerAudioController` | `HandleGroundedChanged(bool)` |
| `OnJumped` | `PlayerAudioController` | `PlayJumpSound()` |
| `OnDashed` | `PlayerAudioController` | `PlayDamageSound()` ⚠️ Bug |
| `OnWallJumped` | `PlayerAudioController` | `PlayJumpSound()` |

### 3.4 Callbacks de Input (PlayerInput → Invoke Unity Events)

| Callback | Acción | Comportamiento |
|----------|--------|---------------|
| `OnJumpInput(ctx)` | Jump | `started` → activa jump buffer + `_jumpHeld = true`. `canceled` → `_jumpHeld = false` |
| `OnDashInput(ctx)` | Dash | `started` → `TryStartDash()` |
| `OnScanInput(ctx)` | Scan | `started` → `_isScanning = true`. `canceled` → `_isScanning = false` |

---

## 4. Sistemas de movimiento

### 4.1 Carrera (HandleRun)

```
No ejecuta si _isDashing o _wallJumpInputLockTimer > 0.
Si _isScanning → velocidad horizontal = 0 (rover se detiene).

targetSpeed = _inputX × maxRunSpeed
rate = |targetSpeed| > 0.01 ? accel : decel
  donde accel/decel se reducen por airControlFactor si !isGrounded

_frameVelocity.x = MoveTowards(actual, target, rate × fixedDeltaTime)
```

**Actualización de facing:** `_facingDir` se actualiza en `HandleRun` cuando `_inputX ≠ 0`. El flip visual lo maneja `PlayerAnimatorController` por separado.

### 4.2 Salto (HandleJumpBuffer + ExecuteJump + HandleJump)

**Buffer + Coyote:**
```
canJump = (isGrounded || coyoteTimer > 0) && !jumpConsumed
Si jumpBufferTimer > 0 && canJump → ExecuteJump()
```

**Ejecución:**
```
_frameVelocity.y = jumpForce
Resetea: coyoteTimer, jumpBufferTimer
Marca: jumpConsumed = true
Dispara: OnJumped
```

**Jump cut (salto corto):**
```
Si !jumpHeld && velocityY > 0 && !isGrounded:
  _frameVelocity.y se decrementa gradualmente
  Factor: (1 - jumpCutMultiplier) × dt × 15
```

> ℹ️ La implementación actual del jump cut usa `MoveTowards` con una velocidad proporcional, no la multiplicación directa del GDD (`velocityY × jumpCutMultiplier`). El resultado es un decaimiento más suave.

### 4.3 Dash (TryStartDash + HandleDash + DashSleep)

**Condición para iniciar:**
```
canDash = (isGrounded || hasAerialDash) && dashCooldownTimer <= 0
```

**Ejecución:**
```
_isDashing = true
dashDurationTimer = dashDuration
dashCooldownTimer = dashCooldown
Si !isGrounded → hasAerialDash = false (consume el dash aéreo)
Inicia coroutine DashSleep (Time.timeScale = 0 por dashSleepTime)
Dispara: OnDashed
```

**Cada FixedUpdate mientras isDashing:**
```
Si dashDurationTimer > 0:
  _frameVelocity.x = dir × dashSpeed  (dir = inputX o facingDir)
  _frameVelocity.y = 0                (anula gravedad)
Si dashDurationTimer <= 0:
  _isDashing = false
```

**Restauración del dash aéreo:** Se resetea `_hasAerialDash = true` al aterrizar (en `CheckCollisions`) y al ejecutar un wall jump.

### 4.4 Wall Slide y Wall Jump (HandleWallSlide + ExecuteWallJump)

**Wall Slide:**
```
isWallSliding = isTouchingWall && !isGrounded && velocityY < 0
Si isWallSliding → velocityY se clampea a max(velocityY, wallSlideSpeed)
Dispara: OnWallSliding(bool) si cambia estado
```

**Wall Jump:**
```
Trigger: isWallSliding && jumpBufferTimer > 0

_frameVelocity.x = -wallDir × wallJumpForceX  (empuja lejos de la pared)
_frameVelocity.y = wallJumpForceY
wallJumpInputLockTimer = wallJumpInputLock     (bloquea input horizontal)
hasAerialDash = true                           (restaura dash)
Dispara: OnWallJumped
```

### 4.5 Gravedad (ApplyGravity)

```
Si isDashing → velocityY = 0 (sin gravedad durante dash)
Si isWallSliding → velocityY clamp a wallSlideSpeed

gravityThisFrame = -gravityScale × fixedDeltaTime
Si velocityY < 0 (cayendo):
  gravityThisFrame *= fallGravityMultiplier

velocityY += gravityThisFrame
velocityY = max(velocityY, maxFallSpeed)  (clamp a velocidad terminal)
```

---

## 5. Detección de colisiones

Usa `Physics2D.OverlapBox` en cada `FixedUpdate`:

| Check | Transform | Size | Layer | Resultado |
|-------|-----------|------|-------|-----------|
| Suelo | `_groundCheckPoint` | `(0.7, 0.05)` | `_groundLayer` | `_isGrounded` |
| Pared | `_wallCheckPoint` | `(0.05, 0.8)` | `_wallLayer` | `_isTouchingWall` |

**Al aterrizar** (transición `!grounded → grounded`):
- `_hasAerialDash = true`
- `_jumpConsumed = false`
- Dispara `OnGroundedChanged(true)`

**Al despegar** (transición `grounded → !grounded`):
- `_coyoteTimer = coyoteTime`
- Dispara `OnGroundedChanged(false)`

**Gizmos:** Dibuja wireframes verde (suelo) y azul (pared) en la vista de escena cuando el objeto está seleccionado.

---

## 6. Estado interno — Variables clave

### 6.1 Timers (decrementan en Update con `Time.deltaTime`)

| Timer | Propósito | Valor inicial |
|-------|-----------|---------------|
| `_coyoteTimer` | Ventana post-borde para saltar | `coyoteTime` al dejar suelo |
| `_jumpBufferTimer` | Ventana pre-suelo para registrar salto | `jumpBufferTime` al presionar Jump |
| `_dashCooldownTimer` | Cooldown entre dashes | `dashCooldown` al iniciar dash |
| `_dashDurationTimer` | Duración del dash activo | `dashDuration` al iniciar dash |
| `_wallJumpInputLockTimer` | Bloqueo de input horizontal post wall jump | `wallJumpInputLock` al wall jump |

### 6.2 Flags

| Flag | Propósito |
|------|-----------|
| `_isGrounded` | Resultado del OverlapBox de suelo |
| `_isTouchingWall` | Resultado del OverlapBox de pared |
| `_isWallSliding` | Tocando pared + en aire + cayendo |
| `_isDashing` | Dash activo |
| `_hasAerialDash` | ¿Tiene dash aéreo disponible? |
| `_jumpHeld` | ¿Botón de salto presionado? |
| `_jumpConsumed` | ¿Ya saltó desde este contacto con suelo? |
| `_isScanning` | ¿Manteniendo botón de escaneo? |

---

## 7. Diagrama de flujo del FixedUpdate

```
FixedUpdate()
│
├─ CheckCollisions()
│    ├─ Actualiza _isGrounded, _isTouchingWall
│    ├─ Al aterrizar: restaura dash, resetea jumpConsumed
│    └─ Al despegar: inicia coyoteTimer
│
├─ HandleJumpBuffer()
│    └─ Si jumpBuffer > 0 && (grounded || coyote) → ExecuteJump()
│
├─ HandleWallSlide()
│    ├─ Calcula _isWallSliding
│    └─ Si wallSliding && jumpBuffer > 0 → ExecuteWallJump()
│
├─ HandleDash()
│    └─ Si _isDashing: aplica dashSpeed, anula Y. Si timer ≤ 0: termina
│
├─ HandleJump()
│    └─ Jump cut: si no se mantiene el botón y subiendo → decrementa Y
│
├─ HandleRun()
│    ├─ Skip si isDashing o wallJumpInputLock activo
│    ├─ MoveTowards hacia targetSpeed con accel/decel
│    └─ Si isScanning → velocityX = 0
│
├─ ApplyGravity()
│    ├─ Skip si isDashing (Y = 0)
│    ├─ Si wallSliding: clamp a wallSlideSpeed
│    └─ Aplica gravedad con multiplicador de caída
│
└─ ApplyVelocity()
     └─ rb.linearVelocity = _frameVelocity
```

---

## 8. Extensión futura

### Integración con DegradationSystem (Sprint 2+)

El `DegradationSystem` deberá modificar los parámetros del `RoverStatsSO` según la fase de degradación activa. Opciones de implementación:

1. **ScriptableObject con modificadores:** El `DegradationSystem` aplica multiplicadores sobre una copia runtime del SO.
2. **Propiedades computadas:** Exponer getters en `PlayerController` que lean la fase actual y apliquen modificadores inline.

Los parámetros afectados por degradación (ref. GDD §4.2):

| Fase | Parámetros modificados |
|------|----------------------|
| 3 — AVERÍA | `dashCooldown × 1.5`, `dashesInAir = 0`, delay 0.04 s en dash |
| 4 — CRÍTICO | `maxRunSpeed × 0.75`, `runAcceleration × 0.80`, `jumpBufferTime = 0.06` |
| 5 — EMERGENCIA | `jumpForce × 0.85`, wall jump desactivado, `coyoteTime = 0.06` |
| 6 — EXTINCIÓN | `maxRunSpeed × 0.50`, sin dash, movimiento asimétrico |

### Eventos pendientes de crear

| Evento sugerido | Suscriptor futuro |
|-----------------|-------------------|
| `OnDamageReceived` | `DegradationSystem`, `HUDManager`, `PlayerAudioController` |
| `OnDeath` | `GameManager`, `LevelManager`, `PlayerAudioController` |
| `OnLand` | `PlayerAudioController` (SFX aterrizaje), VFX partículas |
