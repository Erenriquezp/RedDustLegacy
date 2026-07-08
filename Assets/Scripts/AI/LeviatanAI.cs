using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Boss del Nivel 1 — Leviatán (S04 T3, GDD §8.3).
/// FSM: Dormant → Intro (lockdown/emersión) → Idle/Track → Attack → Vulnerable → …
/// → Death. Enrage a &lt;50% HP (tentáculos ×1,5 + un 4º golpe por ciclo).
/// Solo recibe daño en el núcleo (<see cref="LeviatanCore"/>) durante Vulnerable;
/// los tentáculos (<see cref="LeviatanTentacle"/>) son invulnerables.
/// En la arena real: dejar <see cref="startDormant"/> activo y que
/// <see cref="BossArenaTrigger"/> llame a <see cref="StartEncounter"/>.
/// </summary>
public class LeviatanAI : MonoBehaviour
{
    [Header("Stats")]
    public LeviatanStatsSO stats;

    [Header("References")]
    public Animator animator;
    [Tooltip("Hijo con LeviatanCore + Collider2D trigger. Solo activo en Vulnerable.")]
    public GameObject coreHitbox;
    [Tooltip("Colliders de tentáculo (LeviatanTentacle). Se apagan al morir.")]
    public GameObject[] tentacleHitboxes;

    [Header("Target")]
    public Transform rover;

    [Header("Comportamiento")]
    [Tooltip("Distancia centro-a-centro para atacar. El cuerpo sólido mide ~2 u de semiancho: debe ser mayor que eso o nunca entra en rango.")]
    public float attackRange = 4f;
    [Tooltip("Si está activo, espera a StartEncounter() (lo llama BossArenaTrigger). Apagado para probar en sandbox.")]
    public bool startDormant = false;

    [Header("Feedback visual (S05: la ventana vulnerable debe VERSE)")]
    [Tooltip("Sprite del cuerpo. Vacío = se busca en este GameObject.")]
    public SpriteRenderer bodySprite;
    [Tooltip("Tinte del cuerpo mientras el núcleo está expuesto.")]
    public Color vulnerableTint = new Color(0.45f, 1f, 0.9f);
    [Tooltip("Color del flash al recibir daño en el núcleo.")]
    public Color damageFlashColor = new Color(1f, 0.25f, 0.2f);

    /// <summary>HP actual, HP máximo (para la barra del HUD).</summary>
    public event Action<int, int> OnHealthChanged;
    public event Action OnDefeated;

    public bool IsDead => currentState == State.Death;
    public bool IsVulnerable => currentState == State.Vulnerable;

    private EnemyAudioController audioController;
    private HUDManager hud;

    private int currentHp;
    private bool isEnraged;
    private bool extraStrikeDone;   // 4º tentáculo del ciclo en enrage

    private float stateTimer;
    private float attackTimer;
    private float attackCooldown;

    private Color _baseColor;
    private Coroutine _flashRoutine;

    private enum State { Dormant, Intro, Idle, Attack, Vulnerable, Death }
    private State currentState;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (bodySprite == null) bodySprite = GetComponent<SpriteRenderer>();
        _baseColor = bodySprite != null ? bodySprite.color : Color.white;
        audioController = GetComponent<EnemyAudioController>();
        hud = FindFirstObjectByType<HUDManager>(FindObjectsInactive.Include);

        if (rover == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) rover = player.transform;
        }

        currentHp = stats.maxHp;
        attackCooldown = stats.attackCooldown;

        SetCoreExposed(false);

        if (startDormant) currentState = State.Dormant;
        else BeginIntro();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Intro:      UpdateIntro();      break;
            case State.Idle:       UpdateIdle();       break;
            case State.Attack:     UpdateAttack();     break;
            case State.Vulnerable: UpdateVulnerable(); break;
        }
    }

    // ── Intro (lockdown + emersión) ─────────────────────────────────────────

    /// <summary>Arranca el combate. La llama BossArenaTrigger al cerrar la arena.</summary>
    public void StartEncounter()
    {
        if (currentState != State.Dormant) return;
        BeginIntro();
    }

    private void BeginIntro()
    {
        currentState = State.Intro;
        stateTimer = 0f;

        if (Core.AudioManager.Instance != null)
            Core.AudioManager.Instance.SetMusicState(Core.AudioManager.MusicState.Combat);
        if (audioController != null) audioController.PlayAttackSound(); // rugido al emerger

        if (hud != null)
        {
            hud.ShowBossBar("LEVIATÁN");
            hud.UpdateBossBar(1f);
        }
    }

    private void UpdateIntro()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= stats.introDuration)
            currentState = State.Idle;
    }

    // ── Idle / Track ────────────────────────────────────────────────────────

    private void UpdateIdle()
    {
        if (rover == null) return;

        FaceRover();
        float distance = Vector2.Distance(transform.position, rover.position);

        if (distance > attackRange)
        {
            animator.SetBool("IsWalking", true);
            transform.position = Vector2.MoveTowards(
                transform.position, rover.position, stats.moveSpeed * Time.deltaTime);
            return;
        }

        animator.SetBool("IsWalking", false);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            BeginAttack();
        }
    }

    // ── Attack (3 tentáculos alternos; +1 golpe en enrage) ─────────────────

    private void BeginAttack()
    {
        currentState = State.Attack;
        stateTimer = 0f;
        extraStrikeDone = false;

        animator.SetBool("IsWalking", false);
        animator.speed = isEnraged ? stats.enrageSpeedMultiplier : 1f;
        animator.SetTrigger("AttackTrigger");
        if (audioController != null) audioController.PlayAttackSound();
    }

    private void UpdateAttack()
    {
        // Fallback por tiempo: si la animación no llama a FinishAttack(),
        // el ciclo avanza igual y el boss nunca se queda atascado.
        float duration = stats.attackDuration / (isEnraged ? stats.enrageSpeedMultiplier : 1f);
        stateTimer += Time.deltaTime;
        if (stateTimer >= duration) FinishAttack();
    }

    /// <summary>La llama la animación de ataque como AnimationEvent al terminar (o el fallback).</summary>
    public void FinishAttack()
    {
        if (currentState != State.Attack) return;

        // Enrage: un 4º golpe extra por ciclo antes de exponer el núcleo (GDD §8.3).
        if (isEnraged && !extraStrikeDone)
        {
            extraStrikeDone = true;
            stateTimer = 0f;
            animator.SetTrigger("AttackTrigger");
            if (audioController != null) audioController.PlayAttackSound();
            return;
        }

        animator.speed = 1f;
        BeginVulnerable();
    }

    // AnimationEvents de la animación de ataque (uno por golpe de tentáculo).
    public void AttackTentacle1() { if (audioController != null) audioController.PlayAttackSound(); }
    public void AttackTentacle2() { if (audioController != null) audioController.PlayAttackSound(); }
    public void AttackTentacle3() { if (audioController != null) audioController.PlayAttackSound(); }

    // ── Vulnerable (núcleo expuesto) ────────────────────────────────────────

    private void BeginVulnerable()
    {
        currentState = State.Vulnerable;
        stateTimer = 0f;
        SetCoreExposed(true);
    }

    private void UpdateVulnerable()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer >= stats.vulnerableTime)
        {
            SetCoreExposed(false);
            currentState = State.Idle;
        }
    }

    private void SetCoreExposed(bool exposed)
    {
        animator.SetBool("IsVulnerable", exposed);
        if (coreHitbox != null) coreHitbox.SetActive(exposed);

        // Ventana de castigo clásica: con el núcleo expuesto los tentáculos no
        // dañan — dashear al núcleo es seguro; fuera de la ventana, castiga.
        if (tentacleHitboxes != null && currentState != State.Death)
            foreach (var t in tentacleHitboxes)
                if (t != null) t.SetActive(!exposed);

        // Tinte del cuerpo: la ventana de daño tiene que leerse a simple vista.
        if (bodySprite != null && _flashRoutine == null)
            bodySprite.color = exposed ? vulnerableTint : _baseColor;
    }

    // ── Daño y muerte ───────────────────────────────────────────────────────

    /// <summary>Daño al núcleo (lo llama LeviatanCore). Solo cuenta durante Vulnerable.</summary>
    public void TakeCoreDamage(int damage)
    {
        if (IsDead || currentState != State.Vulnerable) return;

        currentHp -= Mathf.RoundToInt(damage * stats.damageMultiplier);
        currentHp = Mathf.Max(0, currentHp);

        if (audioController != null) audioController.PlayDamageSound();
        FlashDamage();
        OnHealthChanged?.Invoke(currentHp, stats.maxHp);
        if (hud != null) hud.UpdateBossBar((float)currentHp / stats.maxHp);

        if (currentHp <= 0) { Die(); return; }
        CheckEnrage();
    }

    private void CheckEnrage()
    {
        if (isEnraged || currentHp > stats.maxHp * stats.enrageThreshold) return;
        isEnraged = true;
        animator.SetBool("IsEnraged", true);
        attackCooldown = stats.attackCooldown / stats.enrageSpeedMultiplier;
    }

    private void Die()
    {
        currentState = State.Death;

        SetCoreExposed(false);
        animator.speed = 1f;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsDead", true);

        // Apagar hitboxes: que el cadáver no dañe ni bloquee.
        if (tentacleHitboxes != null)
            foreach (var t in tentacleHitboxes)
                if (t != null) t.SetActive(false);
        foreach (var col in GetComponents<Collider2D>()) col.enabled = false;

        if (audioController != null) audioController.PlayDeathSound();
        if (hud != null) hud.HideBossBar();

        // GDD §14.1: la muerte del boss abre la cinemática de caída → estado CINEMATIC.
        if (Core.AudioManager.Instance != null)
            Core.AudioManager.Instance.SetMusicState(Core.AudioManager.MusicState.Cinematic);

        // BossArenaTrigger escucha esto para desbloquear la salida / disparar la cinemática.
        OnDefeated?.Invoke();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void FlashDamage()
    {
        if (bodySprite == null) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        bodySprite.color = damageFlashColor;
        yield return new WaitForSeconds(0.12f);
        bodySprite.color = IsVulnerable ? vulnerableTint : _baseColor;
        _flashRoutine = null;
    }

    private void FaceRover()
    {
        if (rover == null) return;
        Vector3 scale = transform.localScale;
        scale.x = rover.position.x < transform.position.x
            ? -Mathf.Abs(scale.x)
            :  Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}
