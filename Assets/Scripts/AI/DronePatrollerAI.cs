using UnityEngine;
public class DronePatrollerAI : MonoBehaviour
{
    public DronePatrollerStatsSO stats;
    public Transform rover;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Terrestre")]
    [Tooltip("Capas de suelo/pared sobre las que se apoya y choca. Si se deja vacío, usa Ground + Platform.")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Ajuste fino de altura sobre el suelo (+ sube, − baja).")]
    [SerializeField] private float groundOffset = 0f;
    [SerializeField] private float groundRayLength = 4f;
    private float _footOffset;   // medio alto del sprite: apoya los "pies" en el suelo
    private float _halfWidth;    // medio ancho del sprite: para detectar paredes desde el centro

    private Animator animator;

    private int _patrolDir = 1;  // +1 derecha, −1 izquierda
    private float _originX;       // punto de aparición: centro del área de patrulla

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
        _originX = transform.position.x;

        // Apoyo terrestre: distancia del pivote al borde inferior del sprite (independiente
        // del pivote), para apoyar los "pies" exactos en el suelo. El medio ancho sirve para
        // lanzar el rayo de pared desde el borde real del sprite (los bounds ya incluyen la escala).
        var sr = GetComponent<SpriteRenderer>();
        _footOffset = sr != null ? transform.position.y - sr.bounds.min.y : 0.5f;
        _halfWidth = sr != null ? sr.bounds.extents.x : 0.5f;
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

    // ¿Hay una pared al frente en la dirección dir (+1/−1)? Rayo horizontal desde el centro,
    // a la altura del torso, hasta el borde del sprite + el margen configurado.
    bool IsWallAhead(int dir)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.1f;
        float length = _halfWidth + stats.wallCheckDistance;
        return Physics2D.Raycast(origin, Vector2.right * dir, length, groundLayer).collider != null;
    }

    // ¿Hay suelo justo delante de los pies en la dirección dir? Si no, es un borde: hay que girar.
    bool IsGroundAhead(int dir)
    {
        Vector2 origin = (Vector2)transform.position
                         + Vector2.right * dir * (_halfWidth + stats.ledgeCheckDistance)
                         + Vector2.up * 0.1f;
        return Physics2D.Raycast(origin, Vector2.down, groundRayLength, groundLayer).collider != null;
    }

    // Avanzar en dir solo si el camino está despejado (sin pared y con suelo delante).
    bool CanAdvance(int dir) => !IsWallAhead(dir) && IsGroundAhead(dir);

    // ¿Avanzar en dir sacaría al dron de su área de patrulla?
    bool BeyondPatrolRange(int dir)
    {
        if (stats.patrolRange <= 0f) return false;
        return (transform.position.x - _originX) * dir >= stats.patrolRange;
    }

    #region STATES

    void PatrolState(float distance)
    {
        if (distance < stats.detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        // Gira al toparse con una pared, llegar a un borde o salir del área de patrulla.
        if (!CanAdvance(_patrolDir) || BeyondPatrolRange(_patrolDir))
            _patrolDir = -_patrolDir;

        // Solo avanza si la nueva dirección es segura (evita empujarse contra una esquina).
        if (CanAdvance(_patrolDir))
            MoveHorizontallyTowards(transform.position.x + _patrolDir, stats.patrolSpeed);
    }

    void ChaseState(float distance)
    {
        // Persigue al rover en X, pero nunca atraviesa paredes ni se lanza al vacío.
        int dir = rover.position.x >= transform.position.x ? 1 : -1;
        if (CanAdvance(dir))
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

        // Regresa hacia el centro del área de patrulla, respetando paredes y bordes.
        int dir = _originX >= transform.position.x ? 1 : -1;
        if (CanAdvance(dir))
            MoveHorizontallyTowards(_originX, stats.patrolSpeed);

        // Al llegar (o si una pared/borde le impide seguir), retoma la patrulla.
        if (Mathf.Abs(transform.position.x - _originX) < 0.2f || !CanAdvance(dir))
        {
            _patrolDir = dir;
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

    void Flip()
    {
        // En patrulla/retorno mira hacia donde camina; persiguiendo/atacando mira al rover.
        float dir = (currentState == State.Patrol || currentState == State.Return)
            ? _patrolDir
            : rover.position.x - transform.position.x;

        if (Mathf.Approximately(dir, 0f))
            return;

        Vector3 scale = transform.localScale;
        scale.x = dir > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
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