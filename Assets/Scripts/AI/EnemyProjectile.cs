using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float lifetime;

    public void Initialize(
        Vector2 dir,
        float projectileSpeed,
        float projectileLifetime)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        lifetime = projectileLifetime;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // GDD §4.3: impacto de proyectil = 8–12 SI.
            DegradationSystem degradation =
                other.GetComponentInParent<DegradationSystem>();

            if (degradation != null)
                degradation.TakeDamage(Random.Range(8, 13), transform.position);

            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            Destroy(gameObject);
    }
}