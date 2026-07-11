using System;
using UnityEngine;

/// <summary>
/// Centinela Principal — boss final (S05 T3, GDD §8.7). FSM Idle → Chase →
/// Attack → Dead, anclado al centro de la arena (maxDistanceFromCenter).
/// Fase 1: abanico de 3 BossProjectile (12 SI). Fase 2 (al 50% de HP): abanico
/// de 5 (18 SI) + 1 proyectil rastreador + invoca un Centinela Secundario.
/// Recibe daño del dash ofensivo del rover (patrón S03) y reporta su vida a la
/// barra de boss del HUDManager. Al morir dispara OnDefeated (victoria, T4).
/// </summary>
public class CentinelaPrincipalAI : MonoBehaviour
{
    public CentinelaPrincipalStatsSO stats;
    public Transform rover;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private HUDManager hudManager;

    [SerializeField]
    private GameObject secondaryPrefab;

    [SerializeField]
    private Transform secondarySpawnPoint;

    /// <summary>Se dispara al morir el boss — engancha aquí la victoria (T4).</summary>
    public event Action OnDefeated;

    private Animator animator;
    private EnemyAudioController audioController;

    private Vector3 spawnPosition;
    private Vector3 originalScale;

    private int currentHp;
    private float attackTimer;

    private bool phase2;
    private bool secondarySpawned;
    private bool bossBarShown;

    private const float DashHitCooldown = 0.4f;   // un golpe por dash, no por frame de física
    private float dashHitTimer;

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
        audioController = GetComponent<EnemyAudioController>();

        if (hudManager == null)
            hudManager = FindFirstObjectByType<HUDManager>();

        if (rover == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                rover = player.transform;
        }

        currentHp = stats.maxHp;
        spawnPosition = transform.position;
        originalScale = transform.localScale;
        currentState = State.Idle;
    }

    private void Update()
    {
        if (currentState == State.Dead || rover == null)
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
            ShowBossBar();
            currentState = State.Chase;
        }
    }

    private void UpdateChase()
    {
        FaceRover();

        // Terrestre (no flota): solo se desplaza en X a la altura de su spawn,
        // y nunca más allá de maxDistanceFromCenter de su ancla (GDD §8.7).
        float dir = Mathf.Sign(rover.position.x - transform.position.x);
        float nextX = transform.position.x + dir * stats.moveSpeed * Time.deltaTime;
        bool canAdvance = Mathf.Abs(nextX - spawnPosition.x) <= stats.maxDistanceFromCenter;

        if (canAdvance)
        {
            animator.SetBool("IsMoving", true);
            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
        }
        else
        {
            animator.SetBool("IsMoving", false);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, rover.position);

        // Ataca al llegar al rango — o desde el ancla si no puede acercarse más:
        // es un boss de proyectiles, no necesita el melee. (Antes, si el rover
        // retrocedía >6 u, se quedaba empujando su correa sin volver a atacar.)
        if (distanceToPlayer <= stats.attackRange ||
            (!canAdvance && distanceToPlayer <= stats.detectionRange))
        {
            currentState = State.Attack;
        }
    }

    private void UpdateAttack()
    {
        FaceRover();
        animator.SetBool("IsMoving", false);

        float distance = Vector2.Distance(transform.position, rover.position);

        // Rover fuera del alcance de detección: deja de disparar (evita abanicos
        // infinitos a través de todo el mapa) y espera reposicionándose.
        if (distance > stats.detectionRange)
        {
            currentState = State.Chase;
            return;
        }

        float cooldown = phase2 ? stats.phase2AttackCooldown : stats.attackCooldown;

        if (attackTimer >= cooldown)
        {
            attackTimer = 0f;

            animator.SetTrigger("Attack");

            if (audioController != null)
                audioController.PlayAttackSound();

            Attack();
        }

        // Solo vuelve a perseguir si de verdad puede acercarse (dentro de la correa).
        if (distance > stats.attackRange + 1f && CanAdvanceTowardRover())
        {
            currentState = State.Chase;
        }
    }

    private bool CanAdvanceTowardRover()
    {
        float dir = Mathf.Sign(rover.position.x - transform.position.x);
        float nextX = transform.position.x + dir * 0.2f;
        return Mathf.Abs(nextX - spawnPosition.x) <= stats.maxDistanceFromCenter;
    }

    private void Attack()
    {
        if (phase2)
        {
            // GDD §8.7 F2: abanico ×5 (18 SI) + 1 proyectil de rastreo.
            FireFan(
                stats.phase2Projectiles,
                stats.phase2FanAngle,
                stats.phase2ProjectileDamage,
                stats.projectileSpeed);

            FireTrackingShot();
        }
        else
        {
            // GDD §8.7 F1: abanico ×3 (12 SI), sin seguimiento.
            FireFan(
                stats.fanProjectiles,
                stats.fanAngle,
                stats.projectileDamage,
                stats.projectileSpeed);
        }
    }

    private void FireFan(int bullets, float totalAngle, int damage, float speed)
    {
        if (firePoint == null || stats.projectilePrefab == null || bullets <= 0)
            return;

        // El abanico se centra en la dirección al rover (el flip por escala
        // no rota el firePoint, así que la base se calcula aquí).
        Vector2 toRover =
            ((Vector2)rover.position - (Vector2)firePoint.position).normalized;

        float baseAngle = Mathf.Atan2(toRover.y, toRover.x) * Mathf.Rad2Deg;

        float startAngle = bullets > 1 ? -totalAngle / 2f : 0f;
        float step = bullets > 1 ? totalAngle / (bullets - 1) : 0f;

        for (int i = 0; i < bullets; i++)
        {
            float angle = baseAngle + startAngle + step * i;

            SpawnProjectile(angle, damage, speed, tracking: false);
        }
    }

    private void FireTrackingShot()
    {
        if (firePoint == null || stats.projectilePrefab == null)
            return;

        Vector2 toRover =
            ((Vector2)rover.position - (Vector2)firePoint.position).normalized;

        float angle = Mathf.Atan2(toRover.y, toRover.x) * Mathf.Rad2Deg;

        SpawnProjectile(
            angle,
            stats.phase2ProjectileDamage,
            stats.trackingProjectileSpeed,
            tracking: true);
    }

    private void SpawnProjectile(float angle, int damage, float speed, bool tracking)
    {
        GameObject projectile = Instantiate(
            stats.projectilePrefab,
            firePoint.position,
            Quaternion.Euler(0f, 0f, angle)
        );

        BossProjectile bp = projectile.GetComponent<BossProjectile>();

        if (bp != null)
        {
            bp.speed = speed;
            bp.damage = damage;
            bp.tracking = tracking;
            bp.maxTurnAngle = stats.homingAngle;
        }
    }

    // Daño por dash ofensivo (patrón S03, igual que Biol/Drone/LeviatanCore).
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

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead)
            return;

        // El combate empieza aunque el primer contacto sea un dash por la espalda.
        if (currentState == State.Idle)
        {
            ShowBossBar();
            currentState = State.Chase;
        }

        currentHp = Mathf.Max(currentHp - damage, 0);

        if (hudManager != null)
            hudManager.UpdateBossBar((float)currentHp / stats.maxHp);

        if (audioController != null && currentHp > 0)
            audioController.PlayDamageSound();

        // Fase 2 al llegar al 50% de vida.
        if (!phase2 && currentHp <= stats.maxHp * 0.5f && currentHp > 0)
        {
            EnterPhase2();
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void EnterPhase2()
    {
        phase2 = true;

        animator.SetBool("IsEnraged", true);

        if (!secondarySpawned)
        {
            secondarySpawned = true;
            SpawnSecondary();
        }
    }

    private void SpawnSecondary()
    {
        GameObject prefab = secondaryPrefab != null
            ? secondaryPrefab
            : stats.centinelaSecundarioPrefab;

        if (prefab == null)
            return;

        animator.SetTrigger("Invoke");

        Vector2 spawnPos = secondarySpawnPoint != null
            ? (Vector2)secondarySpawnPoint.position
            : (Vector2)transform.position + Vector2.right * 2f;

        if (AIManager.Instance != null)
            AIManager.Instance.SpawnEnemy(prefab, spawnPos);
        else
            Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    private void Die()
    {
        currentState = State.Dead;

        animator.SetBool("IsMoving", false);
        // Un Attack/Invoke encolado y sin consumir sacaría al animator del
        // estado Dead (ambos entran por Any State).
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Invoke");
        animator.SetTrigger("Dead");

        if (audioController != null)
            audioController.PlayDeathSound();

        if (hudManager != null)
            hudManager.HideBossBar();

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        OnDefeated?.Invoke();

        // Animación Death: 32 frames a 8 fps = 4 s, no interrumpible (GDD §13).
        enabled = false;
        Destroy(gameObject, 4f);
    }

    private void ShowBossBar()
    {
        if (bossBarShown || hudManager == null)
            return;

        bossBarShown = true;
        hudManager.ShowBossBar("Centinela Principal");
        hudManager.UpdateBossBar((float)currentHp / stats.maxHp);
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

    private void OnDrawGizmosSelected()
    {
        if (stats == null)
            return;

        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;

        // Anclaje al centro de la arena
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, stats.maxDistanceFromCenter);

        // Detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRange);
    }
}
