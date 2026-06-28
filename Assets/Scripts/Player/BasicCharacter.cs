using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BasicCharacter : MonoBehaviour
    {
        private static readonly int WalkProperty = Animator.StringToHash("Walk");

        [SerializeField] private float speed = 2f;

        private Animator animator;
        private Rigidbody physicsBody;
        private SpriteRenderer spriteRenderer;

        private Vector2 moveInput;

        private void Awake()
        {
            physicsBody = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // ✅ ESTO ES LO QUE TU INPUT SYSTEM LLAMA
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        private void Update()
        {
            // Flip sprite
            if (spriteRenderer != null)
            {
                if (moveInput.x > 0.01f)
                    spriteRenderer.flipX = false;
                else if (moveInput.x < -0.01f)
                    spriteRenderer.flipX = true;
            }

            // Animación caminar
            if (animator != null)
            {
                animator.SetBool(WalkProperty, moveInput.sqrMagnitude > 0.01f);
            }
        }

        private void FixedUpdate()
        {
            Transform cam = Camera.main.transform;

            Vector3 forward = cam.forward;
            Vector3 right = cam.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            Vector3 move = right * moveInput.x + forward * moveInput.y;

            physicsBody.linearVelocity = new Vector3(
                move.x * speed,
                physicsBody.linearVelocity.y,
                move.z * speed
            );
        }
    }
}