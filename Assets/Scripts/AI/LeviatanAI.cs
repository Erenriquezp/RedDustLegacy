using UnityEngine;

public class LeviatanAI : MonoBehaviour
{
    [Header("Stats")]
    public LeviatanStatsSO stats;

    [Header("References")]
    public Animator animator;
    public GameObject coreHitbox;

    [Header("Target")]
    public Transform rover;

    private float attackTimer;

    private int currentHp;

    private bool isDead;
    private bool isVulnerable;
    private bool isEnraged;

    private float timer;
    private float attackPause = 2f;
    private float attackCooldown;

    private enum State
    {
        Intro,
        Idle,
        Attack,
        Vulnerable,
        Death
    }

    private State currentState;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        currentHp = stats.maxHp;
        attackCooldown = stats.attackCooldown;

        currentState = State.Idle;

        if (coreHitbox != null)
            coreHitbox.SetActive(false);
    }

    private void Update()
    {
        if (isDead)
            return;

        CheckEnrage();

        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;

            case State.Attack:
                Attack();
                break;

            case State.Vulnerable:
                Vulnerable();
                break;
        }
    }

    private void Vulnerable()
    {
        isVulnerable = true;

        animator.SetBool("IsVulnerable", true);

        if (coreHitbox != null)
            coreHitbox.SetActive(true);

        timer += Time.deltaTime;

        if (timer >= attackPause)
        {
            timer = 0f;

            isVulnerable = false;

            animator.SetBool("IsVulnerable", false);

            if (coreHitbox != null)
                coreHitbox.SetActive(false);

            currentState = State.Idle;
        }
    }

    private void CheckEnrage()
    {
        if (isEnraged)
            return;

        if (currentHp <= stats.maxHp * stats.enrageThreshold)
        {
            isEnraged = true;

            animator.SetBool("IsEnraged", true);

            attackCooldown /= stats.enrageSpeedMultiplier;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (!isVulnerable)
            return;

        currentHp -= Mathf.RoundToInt(damage * stats.damageMultiplier);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void TakeCoreDamage(int damage)
    {
        if (isDead)
            return;

        if (!isVulnerable)
            return;

        currentHp -= Mathf.RoundToInt(damage * stats.damageMultiplier);

        Debug.Log("Leviatán recibió " + damage + " de daño.");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        currentState = State.Death;

        animator.SetBool("IsDead", true);

        Debug.Log("Leviatán derrotado.");

        // TODO:
        // UnlockArena();
        // PlayCutscene();
    }

    private void Idle()
    {
        Track();

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            currentState = State.Attack;
        }
    }

    private void Attack()
    {
        animator.SetTrigger("AttackTrigger");

        currentState = State.Vulnerable;
    }

    private void Track()
    {
        if (rover == null)
            return;

        Vector3 scale = transform.localScale;

        if (rover.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }
}