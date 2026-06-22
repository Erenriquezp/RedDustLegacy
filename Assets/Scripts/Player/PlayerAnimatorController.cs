// Assets/Scripts/Player/PlayerAnimatorController.cs
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimatorController : MonoBehaviour
{
    // ── Hash de parámetros (más rápido que strings) ───────────────────
    private static readonly int _speedHash = Animator.StringToHash("Speed");
    private static readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int _isJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int _isScanningHash = Animator.StringToHash("IsScanning");
    private static readonly int _velocityYHash = Animator.StringToHash("VelocityY");
    private static readonly int _isDashingHash = Animator.StringToHash("IsDashing");
    private static readonly int _isOnWallHash = Animator.StringToHash("IsOnWall");
    // Preparados para Sprint 03 (requieren eventos OnDamageReceived / OnDeath en PlayerController):
    // private static readonly int _isDamagedHash = Animator.StringToHash("IsDamaged");
    // private static readonly int _isDeadHash = Animator.StringToHash("IsDead");
    private RoverStatsSO _stats;

    private Animator _animator;
    private PlayerController _controller;
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();           // ← debe estar en el padre
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _stats = _controller.GetStats();
    }

    private void OnEnable()
    {
        _controller.OnGroundedChanged += HandleGroundedChanged;
        _controller.OnWallSliding += HandleWallSliding;
        // Sprint 03:
        // _controller.OnDamageReceived += HandleDamageReceived;
        // _controller.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        _controller.OnGroundedChanged -= HandleGroundedChanged;
        _controller.OnWallSliding -= HandleWallSliding;
        // Sprint 03:
        // _controller.OnDamageReceived -= HandleDamageReceived;
        // _controller.OnDeath -= HandleDeath;
    }
    private void Update()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        float inputX = _controller.GetMoveInput();
        float normalizedSpeed = Mathf.Abs(_rb.linearVelocity.x) / _stats.maxRunSpeed;
        _animator.SetFloat(_speedHash, normalizedSpeed);

        // Flip basado en input directo — responde inmediatamente
        if (inputX > 0.01f)
            _sprite.flipX = false;
        else if (inputX < -0.01f)
            _sprite.flipX = true;

        // Jump
        _animator.SetBool(_isJumpingHash, !_controller.IsGrounded);
        _animator.SetBool(_isScanningHash, _controller.IsScanning);

        // Velocidad vertical (para futuras transiciones Rise/Fall/Land)
        _animator.SetFloat(_velocityYHash, _rb.linearVelocity.y);

        // Dash
        _animator.SetBool(_isDashingHash, _controller.IsDashing);
    }

    // ── Callbacks desde PlayerController ─────────────────────────────
    private void HandleGroundedChanged(bool isGrounded)
    {
        _animator.SetBool(_isGroundedHash, isGrounded);
    }

    private void HandleWallSliding(bool isOnWall)
    {
        _animator.SetBool(_isOnWallHash, isOnWall);
    }

    // ── Sprint 03 (cuando PlayerController exponga OnDamageReceived / OnDeath) ──
    // private void HandleDamageReceived(float amount) => _animator.SetTrigger(_isDamagedHash);
    // private void HandleDeath() => _animator.SetBool(_isDeadHash, true);
}