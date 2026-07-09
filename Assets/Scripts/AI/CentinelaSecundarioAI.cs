using UnityEngine;

public class CentinelaSecundarioAI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform firePoint;

    public CentinelaSecundarioStatsSO stats;
    public Transform rover;

    private Animator animator;
    private Rigidbody2D rb;


    private float attackTimer;
    private int attackCounter;

    private int currentHp;

    private Vector3 spawnPosition;
    private bool movingUp = true;

    private Vector3 originalScale;

    private enum State
    {
        Patrol,
        Chase,
        Attack,
        Return
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
        spawnPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void Update()
    {
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

    private void Patrol()
    {
        if (CanDetectRover())
        {
            currentState = State.Chase;
            return;
        }

        animator.SetBool("IsMoving", true);

        Vector3 target;

        if (movingUp)
            target = spawnPosition + Vector3.up * stats.patrolDistance;
        else
            target = spawnPosition + Vector3.down * stats.patrolDistance;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            stats.patrolSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            movingUp = !movingUp;
        }
    }
    private void Chase()
    {
        FaceRover();
        animator.SetBool("IsMoving", true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            rover.position,
            stats.patrolSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance <= 1.5f)
        {
            currentState = State.Attack;
            return;
        }

        if (!CanDetectRover())
        {
            currentState = State.Return;
        }
    }

    private void Attack()
    {
        FaceRover();
        animator.SetBool("IsMoving", false);

        if (!CanDetectRover())
        {
            currentState = State.Return;
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= stats.attackCooldown)
        {
            attackTimer = 0f;

            Shoot();

            //  attackCounter++;
        }
    }

    private void Return()
    {
        animator.SetBool("IsMoving", true);

        transform.position = Vector2.MoveTowards(
            transform.position,
            spawnPosition,
            stats.patrolSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, spawnPosition) < 0.1f)
        {
            currentState = State.Patrol;
        }
    }

    private void Shoot()
    {
        if (firePoint == null || stats.projectilePrefab == null || rover == null)
            return;

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
            projectileScript.Initialize(rover.position);
        }

        animator.SetTrigger("Attack1");

        attackCounter++;

        Debug.Log($"Disparo #{attackCounter}");
    }

    private bool CanDetectRover()
    {
        if (rover == null)
            return false;

        float distance = Vector2.Distance(transform.position, rover.position);

        return distance <= stats.detectionRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null)
            return;

        // Patrulla
        Gizmos.color = Color.cyan;

        Vector3 up = transform.position + Vector3.up * stats.patrolDistance;
        Vector3 down = transform.position + Vector3.down * stats.patrolDistance;

        Gizmos.DrawLine(up, down);
        Gizmos.DrawSphere(up, 0.15f);
        Gizmos.DrawSphere(down, 0.15f);

        // Detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.detectionRange);
    }

    private void FaceRover()
    {
        if (rover == null)
            return;

        Vector3 scale = originalScale;

        if (rover.position.x < transform.position.x)
            scale.x = -Mathf.Abs(originalScale.x);
        else
            scale.x = Mathf.Abs(originalScale.x);

        transform.localScale = scale;
    }
}