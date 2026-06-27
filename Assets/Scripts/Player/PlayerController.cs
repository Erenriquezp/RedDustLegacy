// Assets/Scripts/Player/PlayerController.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // ── Datos ─────────────────────────────────────────────────────────────
    [SerializeField] private RoverStatsSO _stats;

    // ── Ground detection ──────────────────────────────────────────────────
    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new(0.7f, 0.05f);
    [SerializeField] private LayerMask _groundLayer;

    // ── Wall detection ────────────────────────────────────────────────────
    [Header("Wall Check")]
    [SerializeField] private Transform _wallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new(0.05f, 0.8f);
    [SerializeField] private LayerMask _wallLayer;

    // ── Eventos públicos ──────────────────────────────────────────────────
    public event Action<bool> OnGroundedChanged;
    public event Action OnJumped;
    public event Action OnDashed;
    public event Action OnWallJumped;
    public event Action<bool> OnWallSliding;
    // Sprint 03 (T1): los dispara DegradationSystem vía NotifyDamageReceived/NotifyDeath.
    public event Action<float> OnDamageReceived;   // cantidad de daño recibido
    public event Action OnDeath;                    // SI = 0
    public event Action OnRevive;                   // respawn desde checkpoint (T4)

    // ── Componentes ───────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private PlayerInput _playerInput;

    // ── Estado interno ────────────────────────────────────────────────────
    private Vector2 _frameVelocity;
    private float _inputX;
    private int _facingDir = 1;

    // Timers
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _dashCooldownTimer;
    private float _dashDurationTimer;
    private float _wallJumpInputLockTimer;
    private float _knockbackLockTimer;

    // Flags
    private bool _isGrounded;
    private bool _isTouchingWall;
    private bool _isWallSliding;
    private bool _isDashing;
    private bool _hasAerialDash;
    private bool _jumpHeld;
    private bool _jumpConsumed;
    private bool _isScanning;

    // ── API pública ───────────────────────────────────────────────────────
    public RoverStatsSO GetStats() => _stats;
    public float GetMoveInput() => _inputX;
    public bool IsGrounded => _isGrounded;
    public bool IsDashing => _isDashing;
    public int FacingDir => _facingDir;
    public bool IsScanning => _isScanning;

    // ── Sprint 03 (T1): hooks para DegradationSystem ──────────────────────
    // El sistema de degradación sustituye el SO por una copia runtime y la modifica.
    public void SetStats(RoverStatsSO stats) => _stats = stats;

    // Modificadores de fase gobernados por DegradationSystem (GDD §4.2).
    public bool AerialDashEnabled { get; set; } = true;  // Fase 3 lo desactiva
    public bool WallJumpEnabled   { get; set; } = true;  // Fase 5 lo desactiva
    public bool DashEnabled       { get; set; } = true;  // Fase 6 lo desactiva

    // Permiten a DegradationSystem notificar a los hermanos (audio/animator/HUD).
    public void NotifyDamageReceived(float amount) => OnDamageReceived?.Invoke(amount);
    public void NotifyDeath() => OnDeath?.Invoke();
    public void NotifyRevive() => OnRevive?.Invoke();

    /// <summary>Anula la velocidad acumulada (respawn desde checkpoint — Sprint 03 T4).</summary>
    public void ResetMotion()
    {
        _frameVelocity = Vector2.zero;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Empuja al rover en sentido contrario a la fuente del daño (un poco, para notar el golpe)
    /// y bloquea el control horizontal durante un instante. Lo llama DegradationSystem al recibir daño.
    /// </summary>
    public void ApplyKnockback(Vector2 sourcePosition)
    {
        float dirX = Mathf.Sign(transform.position.x - sourcePosition.x);
        if (dirX == 0f) dirX = -_facingDir;   // si está justo encima, retrocede según el facing

        _frameVelocity = new Vector2(dirX * _stats.knockbackForceX, _stats.knockbackForceY);
        _knockbackLockTimer = _stats.knockbackDuration;
        _isDashing = false;   // un golpe corta el dash
    }

    // ═════════════════════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation = RigidbodyInterpolation2D.None;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.inertia = 1f;   // ← forzar inercia
        _rb.WakeUp();                      // ← despertar explícitamente
        _hasAerialDash = true;
    }

    private void Update()
    {
        TickTimers();
        ReadInput();
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleJumpBuffer();
        HandleWallSlide();
        HandleDash();
        HandleJump();
        HandleRun();
        ApplyGravity();
        ApplyVelocity();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Input

    private void ReadInput()
    {
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        float k = 0f;
        if (keyboard != null)
        {
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) k = 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) k = -1f;
        }

        float g = gamepad != null ? gamepad.leftStick.x.ReadValue() : 0f;
        _inputX = Mathf.Abs(g) > Mathf.Abs(k) ? g : k;
    }

    public void OnJumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _jumpBufferTimer = _stats.jumpBufferTime;
            _jumpHeld = true;
        }
        else if (ctx.canceled)
        {
            _jumpHeld = false;
        }
    }

    public void OnDashInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started) TryStartDash();
    }
    public void OnScanInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)  _isScanning = true;
        if (ctx.canceled) _isScanning = false;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Timers

    private void TickTimers()
    {
        float dt = Time.deltaTime;
        if (_coyoteTimer > 0f) _coyoteTimer -= dt;
        if (_jumpBufferTimer > 0f) _jumpBufferTimer -= dt;
        if (_dashCooldownTimer > 0f) _dashCooldownTimer -= dt;
        if (_dashDurationTimer > 0f) _dashDurationTimer -= dt;
        if (_wallJumpInputLockTimer > 0f) _wallJumpInputLockTimer -= dt;
        if (_knockbackLockTimer > 0f) _knockbackLockTimer -= dt;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Collision Detection

    private void CheckCollisions()
    {
        bool wasGrounded = _isGrounded;

        _isGrounded = Physics2D.OverlapBox(
            _groundCheckPoint.position, _groundCheckSize, 0f, _groundLayer);

        _isTouchingWall = Physics2D.OverlapBox(
            _wallCheckPoint.position, _wallCheckSize, 0f, _wallLayer);

        if (!wasGrounded && _isGrounded)
        {
            _hasAerialDash = true;
            _jumpConsumed = false;
            OnGroundedChanged?.Invoke(true);
        }
        else if (wasGrounded && !_isGrounded)
        {
            _coyoteTimer = _stats.coyoteTime;
            OnGroundedChanged?.Invoke(false);
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Run

    private void HandleRun()
    {
        if (_isDashing) return;
        if (_wallJumpInputLockTimer > 0f) return;
        if (_knockbackLockTimer > 0f) return;   // durante el knockback no se controla el horizontal

        float targetSpeed = _inputX * _stats.maxRunSpeed;
        float accel = _isGrounded
            ? _stats.groundAcceleration
            : _stats.groundAcceleration * _stats.airControlFactor;
        float decel = _isGrounded
            ? _stats.groundDeceleration
            : _stats.groundDeceleration * _stats.airControlFactor;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel;

        _frameVelocity.x = Mathf.MoveTowards(
            _frameVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);

        if (_inputX != 0f)
            _facingDir = _inputX > 0f ? 1 : -1;

        if (_isScanning) 
        {
            _frameVelocity.x = 0f;
            return;
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Jump

    private void HandleJumpBuffer()
    {
        bool canJump = (_isGrounded || _coyoteTimer > 0f) && !_jumpConsumed;
        if (_jumpBufferTimer > 0f && canJump)
            ExecuteJump();
    }

    private void ExecuteJump()
    {
        _frameVelocity.y = _stats.jumpForce;
        _coyoteTimer = 0f;
        _jumpBufferTimer = 0f;
        _jumpConsumed = true;
        OnJumped?.Invoke();
    }

    private void HandleJump()
    {
        if (!_jumpHeld && _frameVelocity.y > 0f && !_isGrounded)
            _frameVelocity.y = Mathf.MoveTowards(
                _frameVelocity.y, 0f,
                _frameVelocity.y * (1f - _stats.jumpCutMultiplier) * Time.fixedDeltaTime * 15f);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Gravity

    private void ApplyGravity()
    {
        if (_isDashing)
        {
            _frameVelocity.y = 0f;
            return;
        }

        if (_isWallSliding)
        {
            _frameVelocity.y = Mathf.Max(_frameVelocity.y, _stats.wallSlideSpeed);
            return;
        }

        float gravityThisFrame = -_stats.gravityScale * Time.fixedDeltaTime;

        if (_frameVelocity.y < 0f)
            gravityThisFrame *= _stats.fallGravityMultiplier;

        _frameVelocity.y += gravityThisFrame;
        _frameVelocity.y = Mathf.Max(_frameVelocity.y, _stats.maxFallSpeed);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Dash

    private void TryStartDash()
    {
        if (!DashEnabled) return;                              // Fase 6: sin dash
        bool aerialOk = _hasAerialDash && AerialDashEnabled;   // Fase 3: sin dash aéreo
        bool canDash = (_isGrounded || aerialOk) && _dashCooldownTimer <= 0f;
        if (!canDash) return;

        _isDashing = true;
        _dashDurationTimer = _stats.dashDuration;
        _dashCooldownTimer = _stats.dashCooldown;

        if (!_isGrounded)
            _hasAerialDash = false;

        StartCoroutine(DashSleep());
        OnDashed?.Invoke();
    }

    private void HandleDash()
    {
        if (!_isDashing) return;

        if (_dashDurationTimer > 0f)
        {
            float dir = _inputX != 0f ? Mathf.Sign(_inputX) : _facingDir;
            _frameVelocity.x = dir * _stats.dashSpeed;
            _frameVelocity.y = 0f;
        }
        else
        {
            _isDashing = false;
        }
    }

    private System.Collections.IEnumerator DashSleep()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(_stats.dashSleepTime);
        Time.timeScale = 1f;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Wall Slide & Wall Jump

    private void HandleWallSlide()
    {
        bool wasWallSliding = _isWallSliding;
        _isWallSliding = _isTouchingWall && !_isGrounded && _frameVelocity.y < 0f;

        if (wasWallSliding != _isWallSliding)
            OnWallSliding?.Invoke(_isWallSliding);

        if (_isWallSliding && _jumpBufferTimer > 0f && WallJumpEnabled)  // Fase 5: sin wall-jump
            ExecuteWallJump();
    }

    private void ExecuteWallJump()
    {
        int wallDir = _isTouchingWall ? _facingDir : -_facingDir;
        _frameVelocity.x = -wallDir * _stats.wallJumpForceX;
        _frameVelocity.y = _stats.wallJumpForceY;

        _wallJumpInputLockTimer = _stats.wallJumpInputLock;
        _jumpBufferTimer = 0f;
        _isWallSliding = false;
        _hasAerialDash = true;

        OnWallJumped?.Invoke();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Apply

    private void ApplyVelocity()
    {
        _rb.WakeUp();
        _rb.linearVelocity = _frameVelocity;
    }

    #endregion

    // ── Gizmos ────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (_groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        }
        if (_wallCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_wallCheckPoint.position, _wallCheckSize);
        }
    }
}