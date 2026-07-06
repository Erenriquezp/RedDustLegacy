using UnityEngine;

public class DroneDetectorAI : MonoBehaviour
{
    [Header("Configuración")]
    public DroneDetectorStatsSO stats;
    public Transform rover;

    [Header("Detección")]
    public LayerMask obstacleMask;

    private Animator animator;
    private Rigidbody2D rb;

    private int currentHp;

    private float alertTimer;
    private Vector2 lastKnownPosition;
    private Vector3 originalScale;

    private float searchTimer;

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
    }

    private void Update()
    {
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
        if (CanSeeRover())
        {
            currentState = State.Alert;
            alertTimer = 0f;

            Debug.Log("Entrando a ALERT");
        }
    }

    private void Alert()
    {
        FaceRover();

        if (CanSeeRover())
        {
            alertTimer += Time.deltaTime;

            if (alertTimer >= stats.alertTime)
            {
                currentState = State.Chase;
                lastKnownPosition = rover.position;

                Debug.Log("Entrando a CHASE");
            }
        }
        else
        {
            currentState = State.Patrol;
            alertTimer = 0f;

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

        lastKnownPosition = rover.position;

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance <= stats.attackDistance)
        {
            currentState = State.Attack;
            Debug.Log("Entrando a ATTACK");
            return;
        }

        if (!CanSeeRover())
        {
            currentState = State.Search;
            searchTimer = stats.searchDuration;

            Debug.Log("Entrando a SEARCH");
        }
    }

    private void Attack()
    {
        FaceRover();

        Debug.Log("Atacando");

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance > stats.attackDistance)
        {
            currentState = State.Chase;
        }
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
            Debug.Log("Volviendo a PATROL");
        }
    }

    private bool CanSeeRover()
    {
        if (rover == null)
            return false;

        Vector2 directionToRover = (rover.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, rover.position);

        // Verificar distancia
        if (distance > stats.visionDistance)
            return false;

        // Dirección hacia donde mira el drone
        Vector2 forward = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        // Verificar ángulo de visión
        float angle = Vector2.Angle(forward, directionToRover);

        if (angle > stats.visionAngle / 2f)
            return false;

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