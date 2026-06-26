using UnityEngine;
public class DronePatrollerAI : MonoBehaviour
{
    public DronePatrollerStatsSO stats;
    public Transform rover;

    [Header("Patrol")]
    public Transform[] waypoints;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    private Animator animator;

    private int currentWaypoint;

    private float attackTimer;
    private float lostPlayerTimer;

    private int currentHp;
    private bool isDead;

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Return
    }

    private State currentState;

    private void Start()
    {
        animator = GetComponent<Animator>();

        currentHp = stats.maxHp;
        currentState = State.Patrol;
    }

    private void Update()
    {
        if (isDead)
            return;

        attackTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, rover.position);

        switch (currentState)
        {
            case State.Patrol:
                PatrolState(distance);
                break;

            case State.Chase:
                ChaseState(distance);
                break;

            case State.Attack:
                AttackState(distance);
                break;

            case State.Return:
                ReturnState(distance);
                break;
        }

        animator.SetFloat("Speed",
            currentState == State.Attack ? 0 : 1);

        Flip();
    }

    #region STATES

    void PatrolState(float distance)
    {
        if (distance < stats.detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        if (waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypoint];

        transform.position = Vector2.MoveTowards(
    transform.position,
    target.position,
    stats.patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            currentWaypoint =
                (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void ChaseState(float distance)
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            rover.position,
            stats.chaseSpeed * Time.deltaTime);

        if (distance <= stats.attackRange)
        {
            currentState = State.Attack;
            return;
        }

        if (distance > stats.detectionRange)
        {
            lostPlayerTimer += Time.deltaTime;

            if (lostPlayerTimer >= stats.loseTime)
            {
                currentState = State.Return;
                lostPlayerTimer = 0f;
            }
        }
        else
        {
            lostPlayerTimer = 0f;
        }
    }

    void AttackState(float distance)
    {
        if (distance > stats.attackRange)
        {
            animator.SetBool("IsAttacking", false);
            currentState = State.Chase;
            return;
        }

        if (attackTimer >= stats.attackCooldown)
        {
            animator.SetBool("IsAttacking", true);

            Shoot();

            attackTimer = 0f;

            Invoke(nameof(StopAttackAnimation), 0.2f);
        }
    }

    void StopAttackAnimation()
    {
        animator.SetBool("IsAttacking", false);
    }

    void ReturnState(float distance)
    {
        if (distance < stats.detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        int nearestWaypoint = GetNearestWaypoint();

        transform.position = Vector2.MoveTowards(
            transform.position,
            waypoints[nearestWaypoint].position,
            stats.patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(
                transform.position,
                waypoints[nearestWaypoint].position) < 0.2f)
        {
            currentWaypoint = nearestWaypoint;
            currentState = State.Patrol;
        }
    }

    #endregion

    void Shoot()
{
    if (projectilePrefab == null)
    {
        Debug.LogWarning("Projectile Prefab no asignado.");
        return;
    }

    if (firePoint == null)
    {
        Debug.LogWarning("FirePoint no asignado.");
        return;
    }

    GameObject bullet = Instantiate(
        projectilePrefab,
        firePoint.position,
        Quaternion.identity);

    EnemyProjectile projectile = bullet.GetComponent<EnemyProjectile>();

    Vector2 dir = (rover.position - firePoint.position).normalized;

    projectile.Initialize(
        dir,
        stats.projectileSpeed,
        stats.projectileLifetime);
}

    int GetNearestWaypoint()
    {
        int nearest = 0;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector2.Distance(
                transform.position,
                waypoints[i].position);

            if (d < minDistance)
            {
                minDistance = d;
                nearest = i;
            }
        }

        return nearest;
    }

    void Flip()
    {
        float dir = rover.position.x - transform.position.x;

        Vector3 scale = transform.localScale;

        if (dir > 0)
            scale.x = Mathf.Abs(scale.x);
        else
            scale.x = -Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        animator.SetBool("IsDead", true);

        Invoke(nameof(DisableEnemy), 1.5f);
    }

    void DisableEnemy()
    {
        gameObject.SetActive(false);
    }
}