using UnityEngine;

/// <summary>
/// Centinela Secundario (S05 T3, GDD §8.6 — revisado: TERRESTRE, ya no flota).
/// Camina con gravedad real (Rigidbody2D + linearVelocity): patrulla en vaivén
/// alrededor de su origen girando en paredes y bordes, persigue agresivamente al
/// rover por el suelo y dispara CentinelaProjectile en cuanto entra en rango
/// (primer disparo inmediato, cada 3er disparo con rastreo parcial). Una vez en
/// persecución no suelta al rover hasta loseRange. Recibe daño del dash (S03).
/// Spawneable por el Centinela Principal: busca al rover por tag al arrancar.
/// </summary>
public class CentinelaSecundarioAI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Capas de suelo/pared para caminar. Vacío = Ground + Platform.")]
    [SerializeField] private LayerMask groundMask;

    public CentinelaSecundarioStatsSO stats;
    public Transform rover;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private EnemyAudioController audioController;

    private float attackTimer;
    private int attackCounter;
    private int currentHp;

    private Vector3 spawnPosition;
    private int patrolDir = 1;   // +1 derecha, −1 izquierda
    private Vector3 originalScale;

    private const float DashHitCooldown = 0.4f;   // un golpe por dash, no por frame de física
    private float dashHitTimer;

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Return,
        Dead
    }

    private State currentState = State.Patrol;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        audioController = GetComponent<EnemyAudioController>();
    }

    private void Start()
    {
        currentHp = stats.maxHp;
        spawnPosition = transform.position;
        originalScale = transform.localScale;

        if (groundMask == 0) groundMask = LayerMask.GetMask("Ground", "Platform");

        // Al ser invocado por el boss (AIManager.SpawnEnemy) nadie asigna el rover.
        if (rover == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                rover = player.transform;
        }
    }

    private void Update()
    {
        if (currentState == State.Dead)
            return;

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;

            case State.Return:
                Return();
                break;
        }
    }

    // ── Locomoción terrestre ────────────────────────────────────────────────

    /// <summary>Camina en X vía física (la gravedad mantiene el contacto con el suelo).</summary>
    private void Walk(float dirX, float speed)
    {
        rb.linearVelocity = new Vector2(dirX * speed, rb.linearVelocity.y);
        animator.SetBool("IsMoving", Mathf.Abs(dirX) > 0.01f);
        if (Mathf.Abs(dirX) > 0.01f) FaceDirection(dirX > 0 ? 1 : -1);
    }

    private void Stop()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetBool("IsMoving", false);
    }

    /// <summary>Pared delante (a media altura del cuerpo).</summary>
    private bool WallAhead(int dir)
    {
        float reach = bodyCollider.bounds.extents.x + 0.2f;
        return Physics2D.Raycast(bodyCollider.bounds.center, Vector2.right * dir, reach, groundMask);
    }

    /// <summary>Hay suelo delante del borde frontal (evita caminar al vacío).</summary>
    private bool GroundAhead(int dir)
    {
        Vector2 origin = new Vector2(
            bodyCollider.bounds.center.x + dir * (bodyCollider.bounds.extents.x + 0.1f),
            bodyCollider.bounds.center.y);
        return Physics2D.Raycast(origin, Vector2.down, bodyCollider.bounds.extents.y + 0.8f, groundMask);
    }

    private bool CanWalkTowards(int dir) => !WallAhead(dir) && GroundAhead(dir);

    // ── Estados ─────────────────────────────────────────────────────────────

    private void Patrol()
    {
        if (CanDetectRover(stats.detectionRange))
        {
            currentState = State.Chase;
            attackTimer = stats.attackCooldown;   // agresivo: primer disparo sin espera
            return;
        }

        // Vaivén horizontal alrededor del origen: gira en paredes, bordes o al
        // llegar al límite del área de patrulla.
        bool beyondRange = (transform.position.x - spawnPosition.x) * patrolDir >= stats.patrolDistance;
        if (beyondRange || !CanWalkTowards(patrolDir))
            patrolDir = -patrolDir;

        Walk(patrolDir, stats.patrolSpeed);
    }

    private void Chase()
    {
        if (rover == null) { currentState = State.Return; return; }

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance <= stats.attackRange)
        {
            currentState = State.Attack;
            return;
        }

        // Lock-on: no suelta al rover hasta loseRange (no al primer parpadeo).
        if (distance > stats.loseRange)
        {
            currentState = State.Return;
            return;
        }

        int dir = rover.position.x > transform.position.x ? 1 : -1;

        // GDD §8.6: no se aleja más de leashRange del origen. En el límite (o
        // ante un precipicio/pared) se planta y sigue disparando desde ahí.
        bool insideLeash =
            Mathf.Abs(transform.position.x + dir * 0.2f - spawnPosition.x) <= stats.leashRange;

        if (insideLeash && CanWalkTowards(dir))
        {
            Walk(dir, stats.chaseSpeed);
        }
        else
        {
            Stop();
            FaceRover();
            currentState = State.Attack;   // bloqueado: dispara desde donde está
        }
    }

    private void Attack()
    {
        if (rover == null) { currentState = State.Return; return; }

        Stop();
        FaceRover();

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance > stats.loseRange)
        {
            currentState = State.Return;
            return;
        }

        // Si el rover se aleja pero sigue enganchado, vuelve a perseguir.
        if (distance > stats.attackRange && CanWalkTowards(rover.position.x > transform.position.x ? 1 : -1))
        {
            currentState = State.Chase;
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= stats.attackCooldown)
        {
            attackTimer = 0f;
            Shoot();
        }
    }

    private void Return()
    {
        int dir = spawnPosition.x > transform.position.x ? 1 : -1;

        // Reenganche inmediato si el rover reaparece en el camino de vuelta.
        if (CanDetectRover(stats.detectionRange))
        {
            currentState = State.Chase;
            return;
        }

        if (Mathf.Abs(transform.position.x - spawnPosition.x) < 0.2f || !CanWalkTowards(dir))
        {
            Stop();
            currentState = State.Patrol;
            return;
        }

        Walk(dir, stats.patrolSpeed);
    }

    private void Shoot()
    {
        if (firePoint == null || stats.projectilePrefab == null || rover == null)
            return;

        attackCounter++;

        // GDD §8.6: cada 3er ataque el proyectil sale con rastreo parcial.
        bool trackingShot =
            stats.trackingShotEvery > 0 &&
            attackCounter % stats.trackingShotEvery == 0;

        GameObject projectile = Instantiate(
            stats.projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        CentinelaProjectile projectileScript =
            projectile.GetComponent<CentinelaProjectile>();

        if (projectileScript != null)
        {
            projectileScript.speed = stats.projectileSpeed;
            projectileScript.lifeTime = stats.projectileLifetime;
            projectileScript.damage = stats.projectileDamage;

            if (trackingShot)
                projectileScript.InitializeTracking(rover, stats.trackingAngle);
            else
                projectileScript.Initialize(rover.position);
        }

        animator.SetTrigger(trackingShot ? "Attack2" : "Attack1");

        if (audioController != null)
            audioController.PlayAttackSound();
    }

    private bool CanDetectRover(float range)
    {
        if (rover == null)
            return false;

        return Vector2.Distance(transform.position, rover.position) <= range;
    }

    // Daño por dash ofensivo (patrón S03, igual que Biol/Drone). Funciona con
    // collider trigger y con collider sólido.
    private void OnTriggerStay2D(Collider2D other)  => DamageOnContact(other);
    private void OnCollisionStay2D(Collision2D c)   => DamageOnContact(c.collider);

    private void DamageOnContact(Collider2D other)
    {
        if (currentState == State.Dead || !other.CompareTag("Player"))
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || !player.IsDashing)
            return;

        if (Time.time - dashHitTimer < DashHitCooldown)
            return;

        dashHitTimer = Time.time;
        TakeDamage(stats.dashDamage);
    }

    public void TakeDamage(int amount)
    {
        if (currentState == State.Dead)
            return;

        currentHp -= amount;

        if (audioController != null && currentHp > 0)
            audioController.PlayDamageSound();

        if (currentHp <= 0)
            Die();
    }

    private void Die()
    {
        currentState = State.Dead;

        Stop();
        // Un ataque encolado y sin consumir sacaría al animator del estado Dead
        // (Attack1/Attack2 entran por Any State).
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.SetTrigger("Dead");

        if (audioController != null)
            audioController.PlayDeathSound();

        if (bodyCollider != null)
            bodyCollider.enabled = false;
        rb.simulated = false;   // el cadáver no sigue empujando ni cayendo

        // El clip CentinelaSecundarioDead dura ~1,8 s: dejarlo terminar
        // + un beat de cadáver antes de desaparecer.
        enabled = false;
        Destroy(gameObject, 3f);
    }

    private void FaceRover()
    {
        if (rover == null)
            return;

        FaceDirection(rover.position.x < transform.position.x ? -1 : 1);
    }

    private void FaceDirection(int dir)
    {
        Vector3 scale = originalScale;
        scale.x = dir < 0 ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null)
            return;

        Vector3 origin = Application.isPlaying ? spawnPosition : transform.position;

        // Área de patrulla (horizontal)
        Gizmos.color = Color.cyan;
        Vector3 left = origin + Vector3.left * stats.patrolDistance;
        Vector3 right = origin + Vector3.right * stats.patrolDistance;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.15f);
        Gizmos.DrawSphere(right, 0.15f);

        // Detección y pérdida
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRange);
        Gizmos.color = new Color(1f, 0.55f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, stats.loseRange);

        // Correa (leash)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, stats.leashRange);
    }
}
