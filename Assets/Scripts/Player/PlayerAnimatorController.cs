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
    private static readonly int _isDamagedHash = Animator.StringToHash("IsDamaged");
    private static readonly int _isDeadHash = Animator.StringToHash("IsDead");
    private const string IdleStateName = "Rover_Idle";   // estado base del RoverAC
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
        _controller.OnDamageReceived += HandleDamageReceived;
        _controller.OnDeath += HandleDeath;
        _controller.OnRevive += HandleRevive;
    }

    private void OnDisable()
    {
        _controller.OnGroundedChanged -= HandleGroundedChanged;
        _controller.OnWallSliding -= HandleWallSliding;
        _controller.OnDamageReceived -= HandleDamageReceived;
        _controller.OnDeath -= HandleDeath;
        _controller.OnRevive -= HandleRevive;
    }
    private void Update()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        float maxSpeed = _stats != null ? _stats.maxRunSpeed : 1f;
        float normalizedSpeed = Mathf.Abs(_rb.linearVelocity.x) / Mathf.Max(0.01f, maxSpeed);
        _animator.SetFloat(_speedHash, normalizedSpeed);

        // Jump
        _animator.SetBool(_isJumpingHash, !_controller.IsGrounded);
        _animator.SetBool(_isScanningHash, _controller.IsScanning);

        // Velocidad vertical (para futuras transiciones Rise/Fall/Land)
        _animator.SetFloat(_velocityYHash, _rb.linearVelocity.y);

        // Dash
        _animator.SetBool(_isDashingHash, _controller.IsDashing);
    }

    /// <summary>
    /// Flip de la dirección. En LateUpdate (corre después de que el Animator evalúa) y
    /// sin depender del animator ni de _stats. Voltea la ESCALA X del transform del sprite
    /// (no `flipX`, que aquí no se reflejaba visualmente) — el mismo método que usa el Biol.
    /// Usa el input en vivo (no depende de los early-returns de HandleRun) y, si no hay
    /// input, conserva el facing del controlador.
    /// </summary>
    private void LateUpdate()
    {
        if (_sprite == null) return;

        float inputX = _controller.GetMoveInput();
        int facing = Mathf.Abs(inputX) > 0.01f
            ? (inputX > 0f ? 1 : -1)
            : _controller.FacingDir;

        _sprite.flipX = false;   // por si quedó marcado de antes; el flip lo hace la escala
        Transform t = _sprite.transform;
        Vector3 s = t.localScale;
        s.x = Mathf.Abs(s.x) * (facing < 0 ? -1f : 1f);
        t.localScale = s;
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

    // ── Sprint 03 — daño / muerte (los dispara DegradationSystem) ──────
    private float _lastDamageAnimTime = -1f;
    private const float DamageAnimCooldown = 0.4f;  // el contacto enemigo daña cada frame de física

    private void HandleDamageReceived(float amount)
    {
        if (Time.time - _lastDamageAnimTime < DamageAnimCooldown) return;
        _lastDamageAnimTime = Time.time;
        _animator.SetTrigger(_isDamagedHash);
    }

    private void HandleDeath() => _animator.SetBool(_isDeadHash, true);

    private void HandleRevive()
    {
        _animator.SetBool(_isDeadHash, false);
        // El estado Rover_Death no tiene transición de salida en el controlador, así que
        // bajar IsDead no basta: forzamos el estado base para no quedar clavados en la
        // pose de muerte tras revivir desde un checkpoint (Sprint 03 T4).
        if (_animator.runtimeAnimatorController != null)
            _animator.Play(IdleStateName, 0, 0f);
    }
}