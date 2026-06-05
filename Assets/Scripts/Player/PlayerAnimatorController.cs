// Assets/Scripts/Player/PlayerAnimatorController.cs
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimatorController : MonoBehaviour
{
    // ── Hash de parámetros (más rápido que strings) ───────────────────
    private static readonly int _speedHash     = Animator.StringToHash("Speed");
    private static readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int _isJumpingHash = Animator.StringToHash("IsJumping");
    private RoverStatsSO _stats;

    private Animator         _animator;
    private PlayerController _controller;
    private Rigidbody2D      _rb;
    private SpriteRenderer   _sprite;

    private void Awake()
    {
        _animator   = GetComponentInChildren<Animator>();
        _controller = GetComponent<PlayerController>();
        _rb         = GetComponent<Rigidbody2D>();           // ← debe estar en el padre
        _sprite     = GetComponentInChildren<SpriteRenderer>();
        _stats = _controller.GetStats();

        // Debug temporal — borra después
        if (_animator == null)
        Debug.LogError("❌ Animator no encontrado en hijos de Player");
        else
            Debug.Log($"✅ Animator encontrado: {_animator.gameObject.name} | Controller: {_animator.runtimeAnimatorController}");

        if (_rb == null)
            Debug.LogError("❌ Rigidbody2D no encontrado en Player");

        if (_stats == null)
            Debug.LogError("❌ RoverStats no encontrado");
    }

    private void OnEnable()
    {
        _controller.OnGroundedChanged += HandleGroundedChanged;
    }

    private void OnDisable()
    {
        _controller.OnGroundedChanged -= HandleGroundedChanged;
    }
   private void Update()
    {
        if (_animator == null || _animator.runtimeAnimatorController == null) return;

        float inputX = _controller.GetMoveInput();
        float normalizedSpeed = Mathf.Abs(inputX) > 0.01f ? 1f : 0f;
        _animator.SetFloat(_speedHash, normalizedSpeed);

        // Flip basado en input directo — responde inmediatamente
        if (inputX > 0.01f)
            _sprite.flipX = false;
        else if (inputX < -0.01f)
            _sprite.flipX = true;

        // Jump
        _animator.SetBool(_isJumpingHash, !_controller.IsGrounded);
    }

    // ── Callback desde PlayerController ──────────────────────────────
    private void HandleGroundedChanged(bool isGrounded)
    {
        _animator.SetBool(_isGroundedHash, isGrounded);
    }
}