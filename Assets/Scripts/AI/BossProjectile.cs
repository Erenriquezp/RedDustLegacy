using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 8f;

    public float lifeTime = 5f;

    public bool tracking = false;

    public float turnSpeed = 45f;

    private Transform rover;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            rover = player.transform;
    }

    private void Update()
{
    Debug.Log($"Speed = {speed}");

    if (tracking && rover != null)
    {
        Vector2 direction =
            (rover.position - transform.position).normalized;

        float targetAngle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float currentAngle = transform.eulerAngles.z;

        float angle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            turnSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    transform.position += transform.right * speed * Time.deltaTime;
}
}