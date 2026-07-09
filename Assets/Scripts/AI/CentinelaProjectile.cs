using UnityEngine;

public class CentinelaProjectile : MonoBehaviour
{
    private Vector2 direction;

    public float speed = 10f;
    public float lifeTime = 5f;

    public void Initialize(Vector2 targetPosition)
    {
        direction = (targetPosition - (Vector2)transform.position).normalized;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}