using UnityEngine;

public class BioluminescentAI : MonoBehaviour
{
    public EnemyStatsSO stats;
    public Transform rover;

    private Animator animator;
    private bool isStunned;
    private float stunTimer;

    private bool isAttacking;
    private float attackCooldown = 1.0f;
    private float attackTimer;
    private int _currentHp;
    private float stunCooldown;

    // ─── LÍNEA NUEVA 1: Referencia a tu controlador de audio ───
    private EnemyAudioController audioController; 

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
        _currentHp = stats.hp;
        currentState = State.Idle;

        // ─── LÍNEA NUEVA 2: Buscamos tu componente en el mismo objeto ───
        audioController = GetComponent<EnemyAudioController>(); 
    }

    private void Update()
    {
        if (stunCooldown > 0)
        {
            stunCooldown -= Time.deltaTime;
        }
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
            {
                isStunned = false;
            }

            return;
        }
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
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

        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance <= stats.alertRange)
        {
            alertTimer = 0f;
            currentState = State.Alert;
            ReportAlert(true);   // S04 T4.2: música → Tension
        }
    }

    private void UpdateAlert()
    {
        animator.SetFloat("Speed", 0);

        Vector3 direction = rover.position - transform.position;

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);

        alertTimer += Time.deltaTime;

        if (alertTimer >= stats.alertTime)
        {
            currentState = State.Chase;
        }
    }

    private void UpdateChase()
    {
        float distance = Vector2.Distance(transform.position, rover.position);
        PlayerController player = rover.GetComponent<PlayerController>();
        if (player != null && player.IsScanning)
        {
            ApplyStun();
        }
        
        // Si está cerca, atacar
        if (distance <= 1.5f && attackTimer <= 0)
        {
            animator.SetTrigger("Attack");

            Debug.Log("Bioluminescente ataca");
            
            // ─── LÍNEA OPCIONAL (Si no usas la Solución 1): Disparar sonido de ataque ───
            if (audioController != null) audioController.PlayAttackSound();

            attackTimer = attackCooldown;

            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(
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
                ReportAlert(false);   // S04 T4.2: sin enemigos en alerta → Exploration
            }
        }
        else
        {
            loseTimer = 0f;
        }
    }

    // Daño por contacto. Funciona con collider trigger (OnTriggerStay2D) y con
    // collider sólido (OnCollisionStay2D), para que el Biol pueda ser físico y dañar a la vez.
    private void OnTriggerStay2D(Collider2D other)   => DamageOnContact(other);
    private void OnCollisionStay2D(Collision2D c)    => DamageOnContact(c.collider);

    private float dashHitCooldown = 0.4f;   // un golpe por dash, no por frame de física
    private float dashHitTimer;

    private void DamageOnContact(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Dash ofensivo: si el rover embiste con dash, el Biol recibe daño y el player NO.
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null && player.IsDashing)
        {
            if (Time.time - dashHitTimer >= dashHitCooldown)
            {
                dashHitTimer = Time.time;
                TakeDamage(stats.dashDamage);
            }
            return;
        }

        // Contacto normal (GDD §4.3): golpe plano espaciado por los i-frames del rover
        // (ya no daño/frame en cascada) + knockback desde la posición del Biol.
        DegradationSystem si = other.GetComponentInParent<DegradationSystem>();
        if (si != null)
            si.TakeDamage(stats.contactDamage, transform.position);
    }

    // S04 T4.2: reporta al AudioManager si este enemigo está en Alert/Chase.
    // OnDisable cubre muerte (enabled=false), Destroy y descarga de escena.
    private void ReportAlert(bool inAlert)
    {
        if (Core.AudioManager.Instance != null)
            Core.AudioManager.Instance.ReportEnemyAlert(this, inAlert);
    }

    private void OnDisable() => ReportAlert(false);

    public void ApplyStun()
    {
        if (stunCooldown > 0)
            return;

        isStunned = true;
        stunTimer = stats.stunDuration;
        stunCooldown = stats.stunDuration;

        Debug.Log("Bioluminescente aturdido");
    }

    public void TakeDamage(int amount)
    {
        _currentHp -= amount;

        Debug.Log($"Biol recibe {amount} daño. HP: {_currentHp}");

        // ─── LÍNEA NUEVA 3: Disparar sonido de recibir daño (¡Súper importante aquí!) ───
        if (audioController != null && _currentHp > 0) audioController.PlayDamageSound();

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Biol muerto");

        animator.SetBool("IsDead", true);

        // ─── LÍNEA NUEVA 4: Apagar loops y disparar alarido de muerte ───
        if (audioController != null) audioController.PlayDeathSound();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        enabled = false;
        Destroy(gameObject, 1.0f);
    }
}