using UnityEngine;

/// <summary>
/// Proyectil del Centinela Secundario (S05 T3, GDD §8.6): recto (10 SI); cada
/// 3er disparo sale con rastreo parcial — gira hacia el rover hasta agotar un
/// presupuesto de giro total (30°). Mismo patrón de impacto que EnemyProjectile:
/// daña vía DegradationSystem y se destruye contra el Player o el suelo.
/// </summary>
public class CentinelaProjectile : MonoBehaviour
{
    [Header("Audio de impacto (opcional)")]
    [SerializeField] private AudioClip impactClip;

    public float speed = 7.5f;
    public float lifeTime = 3f;
    public int damage = 10;

    private const float TurnSpeed = 90f;   // grados/s mientras le queda giro

    private Vector2 direction = Vector2.right;
    private Transform rover;
    private bool tracking;
    private float turnBudget;              // grados de giro restantes

    /// <summary>Disparo recto hacia una posición fija.</summary>
    public void Initialize(Vector2 targetPosition)
    {
        direction = (targetPosition - (Vector2)transform.position).normalized;
        FaceDirection();
    }

    /// <summary>Disparo con rastreo parcial (cada 3er ataque, GDD §8.6).</summary>
    public void InitializeTracking(Transform target, float maxTurnAngle)
    {
        rover = target;
        tracking = true;
        turnBudget = maxTurnAngle;
        Initialize(target.position);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (tracking && rover != null && turnBudget > 0f)
        {
            Vector2 desired =
                ((Vector2)rover.position - (Vector2)transform.position).normalized;

            float angleToTarget = Vector2.SignedAngle(direction, desired);
            float maxStep = Mathf.Min(TurnSpeed * Time.deltaTime, turnBudget);
            float step = Mathf.Clamp(angleToTarget, -maxStep, maxStep);

            direction = Quaternion.Euler(0f, 0f, step) * direction;
            turnBudget -= Mathf.Abs(step);
            FaceDirection();
        }

        transform.position += (Vector3)(direction * (speed * Time.deltaTime));
    }

    private void FaceDirection()
    {
        transform.right = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DegradationSystem degradation =
                other.GetComponentInParent<DegradationSystem>();

            if (degradation != null)
                degradation.TakeDamage(damage, transform.position);

            Impact();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            Impact();
    }

    private void Impact()
    {
        if (impactClip != null)
            AudioSource.PlayClipAtPoint(impactClip, transform.position, 0.6f);

        Destroy(gameObject);
    }
}
