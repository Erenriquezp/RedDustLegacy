using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BasicCharacter : MonoBehaviour
    {
        private static readonly int WalkProperty = Animator.StringToHash("Walk");

        [SerializeField] private float speed = 15f;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody physicsBody;
        private Vector2 moveInput;

        private void Awake()
        {
            physicsBody = GetComponent<Rigidbody>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            physicsBody.freezeRotation = true;
            physicsBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        private void Update()
        {
            if (spriteRenderer != null)
            {
                if (moveInput.x > 0.01f)
                    spriteRenderer.flipX = false;
                else if (moveInput.x < -0.01f)
                    spriteRenderer.flipX = true;
            }

            if (animator != null)
            {
                animator.SetBool(WalkProperty, moveInput.sqrMagnitude > 0.01f);
            }
        }

        private void FixedUpdate()
        {
            if (Camera.main == null) return;

            Transform cam = Camera.main.transform;

            Vector3 forward = cam.forward;
            Vector3 right = cam.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 move = right * moveInput.x + forward * moveInput.y;

            Vector3 velocity = move * speed;
            velocity.y = physicsBody.linearVelocity.y;

            physicsBody.linearVelocity = velocity;
        }

        private void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
        }
    }
}