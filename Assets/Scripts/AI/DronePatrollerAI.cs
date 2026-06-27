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

    [Header("Terrestre")]
    [Tooltip("Capas de suelo sobre las que se apoya. Si se deja vacío, usa Ground + Platform.")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Ajuste fino de altura sobre el suelo (+ sube, − baja).")]
    [SerializeField] private float groundOffset = 0f;
    [SerializeField] private float groundRayLength = 4f;
    private float _footOffset;   // medio alto del sprite: apoya los "pies" en el suelo

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

        // Apoyo terrestre: distancia del pivote al borde inferior del sprite (independiente
        // del pivote), para apoyar los "pies" exactos en el suelo.
        var sr = GetComponent<SpriteRenderer>();
        _footOffset = sr != null ? transform.position.y - sr.bounds.min.y : 0.5f;
        if (groundLayer == 0) groundLayer = LayerMask.GetMask("Ground", "Platform");

        StickToGround();
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

        StickToGround();   // se mantiene apoyado en el suelo (es terrestre, no vuela)
        Flip();
    }

    // Mueve solo en X hacia targetX; la Y la fija StickToGround (no persigue al player en vertical).
    void MoveHorizontallyTowards(float targetX, float speed)
    {
        float newX = Mathf.MoveTowards(transform.position.x, targetX, speed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    // Raycast hacia abajo: apoya los "pies" del dron en el suelo. Si no encuentra suelo, no toca la Y.
    void StickToGround()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer);
        if (hit.collider == null) return;

        Vector3 p = transform.position;
        p.y = hit.point.y + _footOffset + groundOffset;
        transform.position = p;
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

        MoveHorizontallyTowards(target.position.x, stats.patrolSpeed);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.2f)
        {
            currentWaypoint =
                (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void ChaseState(float distance)
    {
        MoveHorizontallyTowards(rover.position.x, stats.chaseSpeed);

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

        MoveHorizontallyTowards(waypoints[nearestWaypoint].position.x, stats.patrolSpeed);

        if (Mathf.Abs(transform.position.x - waypoints[nearestWaypoint].position.x) < 0.2f)
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

    // ── Dash ofensivo: el rover embiste con dash y el dron recibe daño ──────
    private float _dashHitTimer;
    private const float DashHitCooldown = 0.4f;   // un golpe por dash, no por frame

    private void OnTriggerStay2D(Collider2D other)  => TryDashHit(other);
    private void OnTriggerEnter2D(Collider2D other) => TryDashHit(other);
    private void OnCollisionStay2D(Collision2D c)   => TryDashHit(c.collider);

    private void TryDashHit(Collider2D other)
    {
        if (isDead || !other.CompareTag("Player")) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || !player.IsDashing) return;

        if (Time.time - _dashHitTimer < DashHitCooldown) return;
        _dashHitTimer = Time.time;
        TakeDamage(stats.dashDamage);
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