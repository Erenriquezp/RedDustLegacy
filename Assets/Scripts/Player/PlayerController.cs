// Assets/Scripts/Player/PlayerController.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    // ── Datos ────────────────────────────────────────────────────────────
    [SerializeField] private RoverStatsSO _stats;

    // ── Ground detection ─────────────────────────────────────────────────
    [Header("Ground Check")]
    [SerializeField] private Transform  _groundCheckPoint;
    [SerializeField] private Vector2    _groundCheckSize  = new(0.7f, 0.05f);
    [SerializeField] private LayerMask  _groundLayer;

    // ── Wall detection ───────────────────────────────────────────────────
    [Header("Wall Check")]
    [SerializeField] private Transform  _wallCheckPoint;
    [SerializeField] private Vector2    _wallCheckSize    = new(0.05f, 0.8f);
    [SerializeField] private LayerMask  _wallLayer;

    // ── Eventos públicos (desacoplamiento) ───────────────────────────────
    public event Action<bool>  OnGroundedChanged;   // para Animator
    public event Action        OnJumped;
    public event Action        OnDashed;
    public event Action        OnWallJumped;
    public event Action<bool>  OnWallSliding;

    // ── Componentes ──────────────────────────────────────────────────────
    private Rigidbody2D _rb;

    // ── Estado interno ───────────────────────────────────────────────────
    private Vector2 _frameVelocity;
    private float   _inputX;

    // Timers
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _dashCooldownTimer;
    private float _dashDurationTimer;
    private float _wallJumpInputLockTimer;

    // Flags
    private bool _isGrounded;
    private bool _isTouchingWall;
    private bool _isWallSliding;
    private bool _isDashing;
    private bool _hasAerialDash;    // un dash aéreo por salto
    private bool _jumpHeld;
    private bool _jumpConsumed;
    private int  _facingDir = 1;    // 1 = derecha, -1 = izquierda

    // ═════════════════════════════════════════════════════════════════════
    #region Unity Lifecycle
    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void Start()
    {
        _rb.gravityScale        = 0f; // manejamos gravedad manualmente
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.interpolation       = RigidbodyInterpolation2D.Interpolate;
        _rb.constraints         = RigidbodyConstraints2D.FreezeRotation;
        _hasAerialDash          = true;
    }

    private void Update()
    {
        TickTimers();
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
    #region Input (New Input System callbacks)

    // Conectar en el Inspector via PlayerInput component
    public void OnMoveInput(InputAction.CallbackContext ctx)
        => _inputX = ctx.ReadValue<Vector2>().x;

    public void OnJumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            _jumpBufferTimer = _stats.jumpBufferTime;
            _jumpHeld        = true;
        }
        if (ctx.canceled)
            _jumpHeld = false;
    }

    public void OnDashInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started) TryStartDash();
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

        // Restaurar dash aéreo al aterrizar
        if (!wasGrounded && _isGrounded)
        {
            _hasAerialDash  = true;
            _jumpConsumed   = false;
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
    #region Timers

    private void TickTimers()
    {
        float dt = Time.deltaTime;
        if (_coyoteTimer          > 0) _coyoteTimer          -= dt;
        if (_jumpBufferTimer      > 0) _jumpBufferTimer      -= dt;
        if (_dashCooldownTimer    > 0) _dashCooldownTimer    -= dt;
        if (_dashDurationTimer    > 0) _dashDurationTimer    -= dt;
        if (_wallJumpInputLockTimer > 0) _wallJumpInputLockTimer -= dt;
    }
    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Run

    private void HandleRun()
    {
        if (_isDashing) return;
        if (_wallJumpInputLockTimer > 0f) return; // bloqueo post-wall jump

        float targetSpeed = _inputX * _stats.maxRunSpeed;
        float accel       = _isGrounded ? _stats.groundAcceleration : _stats.groundAcceleration * _stats.airControlFactor;
        float decel       = _isGrounded ? _stats.groundDeceleration : _stats.groundDeceleration * _stats.airControlFactor;
        float rate        = Mathf.Abs(targetSpeed) > 0.01f ? accel : decel;

        _frameVelocity.x  = Mathf.MoveTowards(_frameVelocity.x, targetSpeed, rate * Time.deltaTime);

        // Flip sprite
        if (_inputX != 0)
            _facingDir = _inputX > 0 ? 1 : -1;
    }
    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Jump

    private void HandleJumpBuffer()
    {
        // Consume buffer si hay suelo disponible (suelo real o coyote)
        bool canJump = (_isGrounded || _coyoteTimer > 0f) && !_jumpConsumed;
        if (_jumpBufferTimer > 0f && canJump)
            ExecuteJump();
    }

    private void ExecuteJump()
    {
        _frameVelocity.y  = _stats.jumpForce;
        _coyoteTimer       = 0f;
        _jumpBufferTimer   = 0f;
        _jumpConsumed      = true;
        OnJumped?.Invoke();
    }

    private void HandleJump()
    {
        // Variable height: cortar subida al soltar el botón
        if (!_jumpHeld && _frameVelocity.y > 0f)
            _frameVelocity.y *= _stats.jumpCutMultiplier;
    }
    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Gravity

    private void ApplyGravity()
    {
        if (_isDashing)
        {
            _frameVelocity.y = 0f; // gravity scale 0 durante dash
            return;
        }

        if (_isWallSliding)
        {
            _frameVelocity.y = Mathf.Max(_frameVelocity.y, _stats.wallSlideSpeed);
            return;
        }

        float gravity = -_stats.gravityScale;

        // Caída más pesada que subida (game feel)
        if (_frameVelocity.y < 0f)
            gravity *= _stats.fallGravityMultiplier;

        _frameVelocity.y += gravity * Time.deltaTime;
        _frameVelocity.y  = Mathf.Max(_frameVelocity.y, _stats.maxFallSpeed);
    }
    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Dash

    private void TryStartDash()
    {
        bool canDash = (_isGrounded || _hasAerialDash) && _dashCooldownTimer <= 0f;
        if (!canDash) return;

        _isDashing         = true;
        _dashDurationTimer = _stats.dashDuration;
        _dashCooldownTimer = _stats.dashCooldown;

        if (!_isGrounded)
            _hasAerialDash = false;

        // dashSleepTime: congelar física brevemente
        StartCoroutine(DashSleep());
        OnDashed?.Invoke();
    }

    private void HandleDash()
    {
        if (!_isDashing) return;

        if (_dashDurationTimer > 0f)
        {
            // Input de dirección o facing direction
            float dir = _inputX != 0 ? Mathf.Sign(_inputX) : _facingDir;
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

        // Wall Jump: se ejecuta si hay jump buffer activo tocando pared
        if (_isWallSliding && _jumpBufferTimer > 0f)
            ExecuteWallJump();
    }

    private void ExecuteWallJump()
    {
        // Saltar en dirección opuesta a la pared
        int wallDir = _isTouchingWall ? _facingDir : -_facingDir;
        _frameVelocity.x = -wallDir * _stats.wallJumpForceX;
        _frameVelocity.y = _stats.wallJumpForceY;

        _wallJumpInputLockTimer = _stats.wallJumpInputLock;
        _jumpBufferTimer        = 0f;
        _isWallSliding          = false;
        _hasAerialDash          = true; // restaurar dash al wall jump

        OnWallJumped?.Invoke();
    }
    #endregion

    // ═════════════════════════════════════════════════════════════════════
    #region Apply

    private void ApplyVelocity() => _rb.linearVelocity = _frameVelocity;
    #endregion

    // ── Gizmos de debug ──────────────────────────────────────────────────
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