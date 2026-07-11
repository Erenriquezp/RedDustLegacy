using UnityEngine;

public class DroneDetectorAI : MonoBehaviour
{
    [Header("Configuración")]
    public DroneDetectorStatsSO stats;
    public Transform rover;

    [Header("Detección")]
    [Tooltip("Capas que bloquean la visión y la patrulla. Vacío = Ground + Platform.")]
    public LayerMask obstacleMask;

    [Header("Ataque (GDD §8.5: proyectil recto)")]
    public GameObject projectilePrefab;
    [Tooltip("Origen del disparo. Vacío = centro del dron.")]
    public Transform firePoint;

    [Header("Audio")]
    [SerializeField] private EnemyAudioController audioController;

    private Animator animator;
    private Rigidbody2D rb;

    private int currentHp;
    private bool isDead;

    private float alertTimer;
    private float alertLoseTimer;
    private Vector2 lastKnownPosition;
    private Vector3 originalScale;

    private float searchTimer;
    private float attackTimer;

    private int _patrolDir = 1;   // +1 derecha, −1 izquierda
    private float _originX;       // centro del área de patrulla (punto de aparición)

    private enum State
    {
        Patrol,
        Alert,
        Chase,
        Attack,
        Search
    }

    private State currentState = State.Patrol;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHp = stats.maxHp;
        originalScale = transform.localScale;
        _originX = transform.position.x;

        if (obstacleMask == 0) obstacleMask = LayerMask.GetMask("Ground", "Platform");

        // Mismo fallback que el Leviatán: si nadie asignó el rover, se busca por tag.
        if (rover == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) rover = player.transform;
        }
    }

    private void Update()
    {
        if (isDead)
            return;

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Alert:
                Alert();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
                break;

            case State.Search:
                Search();
                break;
        }
    }

    private void Patrol()
    {
        // Patrulla aérea (GDD §8.5, 2,5 u/s): vaivén horizontal alrededor del origen;
        // gira al llegar al límite del área o al toparse con una pared.
        if (WallAhead(_patrolDir) || BeyondPatrolRange(_patrolDir))
            _patrolDir = -_patrolDir;

        transform.position += Vector3.right * (_patrolDir * stats.patrolSpeed * Time.deltaTime);
        FaceDirection(_patrolDir);   // el cono de visión mira hacia donde vuela

        if (CanSeeRover())
        {
            currentState = State.Alert;
            alertTimer = 0f;
            alertLoseTimer = 0f;

            // S05 T3: avisa al AIManager (comunicación de drones) y sube la música a Tension.
            NotifyAIManager();
            ReportAlert(true);

            Debug.Log("Entrando a ALERT");
        }
    }

    private bool WallAhead(int dir)
    {
        return Physics2D.Raycast(transform.position, Vector2.right * dir, 1f, obstacleMask)
            .collider != null;
    }

    private bool BeyondPatrolRange(int dir)
    {
        if (stats.patrolRange <= 0f) return false;
        return (transform.position.x - _originX) * dir >= stats.patrolRange;
    }

    private void Alert()
    {
        FaceRover();

        if (CanSeeRover())
        {
            alertLoseTimer = 0f;
            alertTimer += Time.deltaTime;

            if (alertTimer >= stats.alertTime)
            {
                currentState = State.Chase;
                lastKnownPosition = rover.position;

                // Detección confirmada: refresca la última posición conocida en el AIManager.
                NotifyAIManager();

                Debug.Log("Entrando a CHASE");
            }
        }
        else
        {
            // Gracia antes de rendirse: un salto del rover sacaba el contacto del
            // cono un instante y la alerta se cancelaba en el acto.
            alertLoseTimer += Time.deltaTime;
            if (alertLoseTimer < stats.alertLoseGrace) return;

            currentState = State.Patrol;
            alertTimer = 0f;
            ReportAlert(false);

            Debug.Log("Volviendo a PATROL");
        }
    }

    private void Chase()
    {
        FaceRover();

        transform.position = Vector2.MoveTowards(
            transform.position,
            rover.position,
            stats.moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(transform.position, rover.position);

        // Lock-on: confirmada la detección, persigue mientras el rover esté a
        // menos de chaseRange — sin exigir visión continua (antes cualquier
        // esquina o salto lo mandaba a Search al instante).
        if (distance <= stats.chaseRange)
            lastKnownPosition = rover.position;

        if (distance <= stats.attackDistance)
        {
            currentState = State.Attack;
            attackTimer = stats.attackCooldown;   // primer disparo inmediato
            Debug.Log("Entrando a ATTACK");
            return;
        }

        if (distance > stats.chaseRange)
        {
            currentState = State.Search;
            searchTimer = stats.searchDuration;

            Debug.Log("Entrando a SEARCH");
        }
    }

    private void Attack()
    {
        FaceRover();

        // Proyectil recto 8–12 SI cada attackCooldown (GDD §8.5). El daño lo
        // aplica EnemyProjectile al impactar (mismo prefab que el Patrullero).
        attackTimer += Time.deltaTime;
        if (attackTimer >= stats.attackCooldown)
        {
            attackTimer = 0f;
            Shoot();
        }

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance > stats.attackDistance)
        {
            currentState = State.Chase;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || rover == null)
        {
            if (projectilePrefab == null)
                Debug.LogWarning("[DroneDetectorAI] Projectile Prefab no asignado.");
            return;
        }

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(projectilePrefab, origin, Quaternion.identity);

        if (audioController != null) audioController.PlayAttackSound();

        EnemyProjectile projectile = bullet.GetComponent<EnemyProjectile>();
        Vector2 dir = (rover.position - origin).normalized;
        projectile.Initialize(dir, stats.projectileSpeed, stats.projectileLifetime);
    }

    private void Search()
    {
        FaceRover();

        transform.position = Vector2.MoveTowards(
            transform.position,
            lastKnownPosition,
            stats.moveSpeed * Time.deltaTime
        );

        if (CanSeeRover())
        {
            currentState = State.Chase;
            Debug.Log("Volviendo a CHASE");
            return;
        }

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0)
        {
            currentState = State.Patrol;
            ReportAlert(false);

            // Perdió el rastro: limpia la detección compartida (GDD §8.1).
            if (AIManager.Instance != null)
                AIManager.Instance.ClearDetection();

            Debug.Log("Volviendo a PATROL");
        }
    }

    private bool CanSeeRover()
    {
        if (rover == null)
            return false;

        Vector2 directionToRover = (rover.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, rover.position);

        // Detección por proximidad: muy cerca lo "siente" en cualquier dirección
        // (el cono horizontal era ciego por detrás/arriba/abajo). Las paredes
        // siguen bloqueando (raycast de abajo).
        bool inProximity = distance <= stats.proximityRadius;

        if (!inProximity)
        {
            // Verificar distancia del cono
            if (distance > stats.visionDistance)
                return false;

            // Dirección hacia donde mira el drone
            Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            // Verificar ángulo de visión
            float angle = Vector2.Angle(forward, directionToRover);

            if (angle > stats.visionAngle / 2f)
                return false;
        }

        // Verificar si una pared bloquea la visión
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToRover,
            distance,
            obstacleMask
        );

        if (hit.collider != null)
            return false;

        return true;
    }
    private void OnDrawGizmosSelected()
    {
        if (stats == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, stats.visionDistance);

        Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        Vector3 left = Quaternion.Euler(0, 0, -stats.visionAngle / 2f) * forward;
        Vector3 right = Quaternion.Euler(0, 0, stats.visionAngle / 2f) * forward;

        Gizmos.DrawRay(transform.position, left * stats.visionDistance);
        Gizmos.DrawRay(transform.position, right * stats.visionDistance);

        // Proximidad omnidireccional y rango de persecución
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.proximityRadius);
        Gizmos.color = new Color(1f, 0.55f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, stats.chaseRange);
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

    // ── Comunicación de drones — S05 T3 (GDD §8.1) ──────────────────────────
    // Reporta la posición del rover al AIManager, que la reparte a los
    // Patrulleros a menos de 12 u de este Detector.
    private void NotifyAIManager()
    {
        if (AIManager.Instance != null && rover != null)
            AIManager.Instance.ReportPlayerDetected(rover.position, transform.position);
    }

    // S04 T4.2: reporta al AudioManager si este enemigo sostiene la Tension.
    // OnDisable cubre el SetActive(false) de la muerte y la descarga de escena.
    private void ReportAlert(bool inAlert)
    {
        if (Core.AudioManager.Instance != null)
            Core.AudioManager.Instance.ReportEnemyAlert(this, inAlert);
    }

    private void OnDisable() => ReportAlert(false);

    // ── Dash ofensivo (patrón S03): el rover embiste con dash y recibe daño ──
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
        else
        {
            // [AUDIO] Recibir daño controlado
            if (audioController != null) audioController.PlayDamageSound();
        }
    }

    private void Die()
    {
        isDead = true;

        ReportAlert(false);   // un muerto no sostiene la Tension

        // [AUDIO] Muerte controlada
        if (audioController != null) audioController.PlayDeathSound();

        // Sin animación de muerte aún (el controller no tiene parámetros):
        // pequeño margen para que suene el SFX y se desactiva.
        Invoke(nameof(DisableEnemy), 1.5f);
    }

    private void DisableEnemy()
    {
        gameObject.SetActive(false);
    }
}