using UnityEngine;

public class BioluminescentAI : MonoBehaviour
{
    public EnemyStatsSO stats;
    public Transform rover;

    private Animator animator;
    private bool isStunned;
    private float stunTimer;

    private enum State
    {
        Idle,
        Alert,
        Chase
    }

    private State currentState;

    private float alertTimer;
    private float loseTimer;


    private void Start()
    {
        animator = GetComponent<Animator>();
        currentState = State.Idle;
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
            {
                isStunned = false;
            }

            return;
        }

        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;

            case State.Alert:
                UpdateAlert();
                break;

            case State.Chase:
                UpdateChase();
                break;
        }

    }

    private void UpdateIdle()
    {
        animator.SetFloat("Speed", 0);

        float distance =
            Vector2.Distance(transform.position,
                             rover.position);

        if (distance <= stats.alertRange)
        {
            alertTimer = 0f;
            currentState = State.Alert;
        }
        if (distance <= stats.alertRange)
        {
            Debug.Log("Entrando a Alert");
            alertTimer = 0f;
            currentState = State.Alert;
        }
    }

    private void UpdateAlert()
    {
        animator.SetFloat("Speed", 0);

        Vector3 direction =
            rover.position - transform.position;

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);

        alertTimer += Time.deltaTime;

        if (alertTimer >= stats.alertTime)
        {
            currentState = State.Chase;
        }
        if (alertTimer >= stats.alertTime)
        {
            Debug.Log("Entrando a Chase");
            currentState = State.Chase;
        }
    }

    private void UpdateChase()
    {
        float distance =
            Vector2.Distance(transform.position,
                             rover.position);

        Vector2 nextPosition =
            Vector2.MoveTowards(
                transform.position,
                rover.position,
                stats.moveSpeed * Time.deltaTime);

        transform.position = nextPosition;

        animator.SetFloat("Speed", 1);

        if (distance > stats.loseRange)
        {
            loseTimer += Time.deltaTime;

            if (loseTimer >= stats.loseTime)
            {
                loseTimer = 0f;
                currentState = State.Idle;
            }
        }
        else
        {
            loseTimer = 0f;
        }
        Debug.Log("Persiguiendo");
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(
                $"Bioluminescente causa {stats.contactDamage} daño/seg");
        }
    }
    public void ApplyStun()
    {
        isStunned = true;
        stunTimer = stats.stunDuration;

        Debug.Log("Bioluminescente aturdido");
    }
}
