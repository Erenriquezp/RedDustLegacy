using UnityEngine;

/// <summary>
/// Proyectil del Centinela Principal (S05 T3, GDD §8.7). Avanza a lo largo de
/// su transform.right (la rotación la fija el boss al disparar el abanico).
/// Con tracking activo (proyectil rastreador de F2) gira hacia el rover hasta
/// agotar maxTurnAngle. Daña vía DegradationSystem y se destruye al impactar.
/// </summary>
public class BossProjectile : MonoBehaviour
{
    [Header("Audio de impacto (opcional)")]
    [SerializeField] private AudioClip impactClip;

    public float speed = 8f;
    public float lifeTime = 5f;
    public int damage = 12;
    public bool tracking = false;

    [Tooltip("Presupuesto total de giro del rastreador (GDD §8.7: 45°).")]
    public float maxTurnAngle = 45f;

    private const float TurnSpeed = 90f;   // grados/s mientras le queda giro

    private float turnBudget;
    private Transform rover;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        turnBudget = maxTurnAngle;

        if (tracking)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                rover = player.transform;
        }
    }

    private void Update()
    {
        if (tracking && rover != null && turnBudget > 0f)
        {
            Vector2 desired =
                (rover.position - transform.position).normalized;

            float targetAngle =
                Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;

            float currentAngle = transform.eulerAngles.z;
            float maxStep = Mathf.Min(TurnSpeed * Time.deltaTime, turnBudget);

            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxStep);
            turnBudget -= Mathf.Abs(Mathf.DeltaAngle(currentAngle, newAngle));

            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }

        transform.position += transform.right * (speed * Time.deltaTime);
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
