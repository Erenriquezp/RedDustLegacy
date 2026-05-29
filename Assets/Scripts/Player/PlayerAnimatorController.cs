// Assets/Scripts/Player/PlayerAnimatorController.cs
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerController))]
public class PlayerAnimatorController : MonoBehaviour
{
    // ── Hash de parámetros (más rápido que strings) ───────────────────
    private static readonly int _speedHash     = Animator.StringToHash("Speed");
    private static readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

    private Animator         _animator;
    private PlayerController _controller;
    private Rigidbody2D      _rb;
    private SpriteRenderer   _sprite;

    private void Awake()
    {
        _animator   = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
        _rb         = GetComponent<Rigidbody2D>();
        _sprite     = GetComponentInChildren<SpriteRenderer>();
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
        // Speed: valor absoluto de velocidad horizontal normalizado [0,1]
        float normalizedSpeed = Mathf.Abs(_rb.linearVelocity.x) / 12f; // 12 = maxRunSpeed del SO
        _animator.SetFloat(_speedHash, normalizedSpeed, 0.05f, Time.deltaTime); // dampTime suaviza la transición

        // Flip del sprite según dirección (sin rotar el transform)
        if (_rb.linearVelocity.x != 0f)
            _sprite.flipX = _rb.linearVelocity.x < 0f;

         // Velocidad de animación Run proporcional a la velocidad real
         // (solo afecta cuando está en Run, Idle queda en 1x)
         float speedRatio = Mathf.Abs(_rb.linearVelocity.x) / 12f; // 12 = maxRunSpeed
         _animator.SetFloat(_speedHash, speedRatio, 0.05f, Time.deltaTime);

         // Escala el clip de Run entre 0.8x y 1.2x según la velocidad
         _animator.speed = _animator.GetCurrentAnimatorStateInfo(0).IsName("Run")
            ? Mathf.Lerp(0.8f, 1.2f, speedRatio)
            : 1f;
      }

    // ── Callback desde PlayerController ──────────────────────────────
    private void HandleGroundedChanged(bool isGrounded)
    {
        _animator.SetBool(_isGroundedHash, isGrounded);
    }
}