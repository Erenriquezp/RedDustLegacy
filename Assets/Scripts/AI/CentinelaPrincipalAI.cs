using UnityEngine;

public class CentinelaPrincipalAI : MonoBehaviour
{
    public CentinelaPrincipalStatsSO stats;
    public Transform rover;

    private Animator animator;

    private Vector3 spawnPosition;
    private Vector3 originalScale;

    private int currentHp;

    private float attackTimer;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private HUDManager hudManager;

    private bool phase2 = false;

    [SerializeField]
    private GameObject secondaryPrefab;

    [SerializeField]
    private Transform secondarySpawnPoint;

    private bool secondarySpawned;

    [Header("DDA Settings")]
    [SerializeField] private float difficultyMultiplier = 1f;

    [SerializeField] private float minDifficulty = 0.75f;

    [SerializeField] private float maxDifficulty = 1.25f;

    private int deaths;

    private float survivalTimer;

    private enum State
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    private State currentState;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (hudManager == null)
            hudManager = FindFirstObjectByType<HUDManager>();

        currentHp = stats.maxHp;

        spawnPosition = transform.position;

        originalScale = transform.localScale;

        currentState = State.Idle;

        if (hudManager != null)
            hudManager.UpdateBossBar(1f);
    }
    private void Update()
    {
        if (rover == null)
            return;

        attackTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;
        }
    }
    private void UpdateIdle()
    {
        animator.SetBool("IsMoving", false);

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance <= stats.detectionRange)
        {
            if (hudManager != null)
                hudManager.ShowBossBar("Centinela Principal");

            currentState = State.Chase;
        }
    }
    private void UpdateChase()
    {
        FaceRover();

        Vector3 direction =
            (rover.position - transform.position).normalized;

        Vector3 nextPosition =
            transform.position +
            direction * stats.moveSpeed * Time.deltaTime;

        float distanceFromCenter =
            Vector2.Distance(nextPosition, spawnPosition);

        if (distanceFromCenter <= stats.maxDistanceFromCenter)
        {
            transform.position = nextPosition;
        }

        animator.SetBool("IsMoving", true);

        float distanceToPlayer =
            Vector2.Distance(transform.position, rover.position);

        if (distanceToPlayer <= 5f)
        {
            currentState = State.Attack;
        }
    }

    private void UpdateAttack()
    {
        animator.SetBool("IsMoving", false);

        FaceRover();

        if (attackTimer >= stats.attackCooldown)
        {
            attackTimer = 0;

            animator.SetTrigger("Attack");

            ShootFan();
        }

        float distance =
            Vector2.Distance(transform.position, rover.position);

        if (distance > 6f)
        {
            currentState = State.Chase;
        }
    }

    private void FaceRover()
    {
        Vector3 scale = originalScale;

        if (rover.position.x < transform.position.x)
            scale.x = -Mathf.Abs(originalScale.x);
        else
            scale.x = Mathf.Abs(originalScale.x);

        transform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead)
            return;

        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);

        float normalized = (float)currentHp / stats.maxHp;

        if (hudManager != null)
            hudManager.UpdateBossBar(normalized);

        // Entrar a la Fase 2 al llegar al 50% de vida
        if (!phase2 && currentHp <= stats.maxHp * 0.5f)
        {
            EnterPhase2();
        }

        // Morir cuando la vida llegue a 0
        if (currentHp <= 0)
        {
            currentState = State.Dead;

            animator.SetTrigger("Dead");

            if (hudManager != null)
                hudManager.HideBossBar();
        }
    }

    private void EnterPhase2()
    {
        phase2 = true;

        if (!secondarySpawned)
        {
            secondarySpawned = true;

            if (AIManager.Instance != null)
            {
                AIManager.Instance.SpawnEnemy(
                    secondaryPrefab,
                    secondarySpawnPoint.position);
            }
        }
    }

    private void ShootFan()
{
    int bullets = stats.fanProjectiles;

    float totalAngle = stats.fanAngle;

    float startAngle = -totalAngle / 2f;

    float step = totalAngle / (bullets - 1);

    Vector2 direction =
        (rover.position - firePoint.position).normalized;

    float baseAngle =
        Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    for (int i = 0; i < bullets; i++)
    {
        float angle = startAngle + step * i;

        Quaternion rotation =
            Quaternion.Euler(0, 0, baseAngle + angle);

        GameObject projectile = Instantiate(
            stats.projectilePrefab,
            firePoint.position,
            rotation
        );

        BossProjectile bp = projectile.GetComponent<BossProjectile>();

        if (bp != null)
            bp.speed = stats.projectileSpeed;
    }
}

    private void SpawnSecondary()
    {
        if (secondaryPrefab == null)
            return;

        if (secondarySpawnPoint == null)
            return;

        if (AIManager.Instance != null)
        {
            AIManager.Instance.SpawnEnemy(
                secondaryPrefab,
                secondarySpawnPoint.position);
        }
    }

    private void Attack()
    {
        if (phase2)
            AttackPhase2();
        else
            AttackPhase1();
    }

    private void AttackPhase1()
    {
        FireFan(3, 30f);
    }

    private void AttackPhase2()
    {
        FireFan(5, 45f);
    }

    private void FireFan(int bullets, float angle)
    {
        if (firePoint == null)
            return;

        float startAngle = -angle / 2f;
        float step = angle / (bullets - 1);

        for (int i = 0; i < bullets; i++)
        {
            float currentAngle = startAngle + step * i;

            Quaternion rotation =
                firePoint.rotation *
                Quaternion.Euler(0, 0, currentAngle);

            GameObject projectile = Instantiate(
                stats.projectilePrefab,
                firePoint.position,
                rotation
            );

            BossProjectile bp =
    projectile.GetComponent<BossProjectile>();

            if (bp != null)
            {
                bp.speed = stats.projectileSpeed;

                bp.tracking = phase2;
            }
        }
    }
}