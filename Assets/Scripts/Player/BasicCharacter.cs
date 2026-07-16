using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Scripts
{
    /// <summary>
    /// Controlador del rover en la escena isometrica (interludio N1->N2).
    /// Movimiento 3D relativo a la camara, con soporte de animacion Iso-Run/Idle
    /// y respeto de la pausa del GameManager.
    ///
    /// ANIMACION:
    ///   Si el Animator tiene el parametro float "Speed", lo conduce directamente
    ///   (compatible con RoverAC que usa Speed para Idle<->walk/Iso-Run).
    ///   Como fallback usa el bool "Walk" del Character Animator.
    ///
    /// FLIP:
    ///   En vista isometrica el flip correcto es por el componente X de la
    ///   velocidad en espacio de camara (no el input 2D crudo), para que al
    ///   moverse en diagonal hacia arriba-derecha el sprite mire a la derecha.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BasicCharacter : MonoBehaviour
    {
        // ── Hash de parametros del Animator ───────────────────────────────
        private static readonly int HashSpeed    = Animator.StringToHash("Speed");
        private static readonly int HashWalk     = Animator.StringToHash("Walk");
        private static readonly int HashIsoRun   = Animator.StringToHash("Iso-Run");

        [Header("Movimiento")]
        [SerializeField] private float speed = 15f;
        [Tooltip("Umbral de magnitud de input para considerar que el rover se mueve.")]
        [SerializeField] private float moveThreshold = 0.05f;

        [Header("Referencias (opcional — se resuelven automaticamente)")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // ── Internos ──────────────────────────────────────────────────────
        private Rigidbody    _rb;
        private Vector2      _moveInput;
        private bool         _hasSpeedParam;   // true = RoverAC; false = Character Animator
        private bool         _hasIsoRunState;  // true = puede forzar Iso-Run directamente

        // ─────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            if (animator == null)       animator       = GetComponentInChildren<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            _rb.freezeRotation = true;
            _rb.constraints    = RigidbodyConstraints.FreezeRotation;

            // Detectar que parametros existen en el controller asignado.
            if (animator != null)
            {
                foreach (var param in animator.parameters)
                {
                    if (param.name == "Speed")   _hasSpeedParam  = true;
                    if (param.name == "Iso-Run") _hasIsoRunState = true;
                }
            }
        }

        /// <summary>Recibe el input de movimiento del PlayerInput Component.</summary>
        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        private void Update()
        {
            // Si el juego esta pausado/Game Over, congelar input de movimiento.
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                _moveInput = Vector2.zero;
            }

            UpdateAnimation();
            UpdateFacing();
        }

        private void FixedUpdate()
        {
            // No mover si no hay camara o si el juego no esta en Playing.
            if (Camera.main == null) return;
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                // Frenar suavemente al pausar (conserva la Y para no flotar).
                _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
                return;
            }

            ApplyMovement();
        }

        private void LateUpdate()
        {
            // Evita que el Rigidbody rote el transform en colisiones.
            transform.rotation = Quaternion.identity;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region Movimiento

        private void ApplyMovement()
        {
            Transform cam     = Camera.main.transform;
            Vector3   forward = cam.forward;
            Vector3   right   = cam.right;

            forward.y = 0f; forward.Normalize();
            right.y   = 0f; right.Normalize();

            Vector3 move = right * _moveInput.x + forward * _moveInput.y;

            Vector3 velocity = move * speed;
            velocity.y = _rb.linearVelocity.y;   // conservar gravedad
            _rb.linearVelocity = velocity;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region Animacion

        private void UpdateAnimation()
        {
            if (animator == null) return;

            bool isMoving = _moveInput.sqrMagnitude > moveThreshold * moveThreshold;

            if (_hasSpeedParam)
            {
                // RoverAC: Speed controla Idle <-> Rover_walk / Iso-Run.
                // Pasar la magnitud real del input para que el blend sea suave.
                animator.SetFloat(HashSpeed, isMoving ? _moveInput.magnitude : 0f);
            }
            else
            {
                // Character Animator (fallback): bool Walk.
                animator.SetBool(HashWalk, isMoving);
            }

            // Si el controller expone el parametro "Iso-Run" como bool,
            // activarlo mientras nos movemos para que el estado se reproduzca.
            if (_hasIsoRunState)
                animator.SetBool(HashIsoRun, isMoving);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────
        #region Direccion facial

        private void UpdateFacing()
        {
            if (spriteRenderer == null) return;

            // En isometrico el flip se basa en la velocidad horizontal RELATIVA
            // a la camara: si nos movemos hacia la derecha de la pantalla, miramos
            // a la derecha; si vamos hacia la izquierda, al revés.
            // El input.x < 0 mueve a la izquierda de camara => flip = true.
            if (_moveInput.x > moveThreshold)
                spriteRenderer.flipX = false;
            else if (_moveInput.x < -moveThreshold)
                spriteRenderer.flipX = true;
            // Si solo se mueve en Y (hacia/desde la camara) mantenemos el flip previo.
        }

        #endregion
    }
}
