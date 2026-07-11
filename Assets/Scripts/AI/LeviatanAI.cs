using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Boss del Nivel 1 — Leviatán (S04 T3 + S05, GDD §8.3).
/// FSM: Dormant → Intro (lockdown/emersión) → Idle/Track → Attack → Vulnerable → …
/// → Death. A media/larga distancia usa la EMBESTIDA: telegraph (tinte + rugido)
/// → salto en arco que cae DELANTE del rover (objetivo fijado al despegar, para
/// que se pueda esquivar) → onda de impacto → combo de tentáculos → Vulnerable.
/// Enrage a &lt;50% HP (todo ×1,5 + un 4º golpe por ciclo).
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
    [Tooltip("Tinte del aviso de embestida (el jugador DEBE poder leer que viene el salto).")]
    public Color lungeTelegraphTint = new Color(1f, 0.55f, 0.1f);

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

    // Embestida
    private float lungeCooldownTimer;
    private Vector3 _lungeStart;
    private Vector3 _lungeTarget;   // fijado al despegar: da al jugador el vuelo para esquivar

    private Color _baseColor;
    private Coroutine _flashRoutine;

    private enum State { Dormant, Intro, Idle, LungeTelegraph, Lunge, Attack, Vulnerable, Death }
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
        if (lungeCooldownTimer > 0f) lungeCooldownTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Intro:          UpdateIntro();          break;
            case State.Idle:           UpdateIdle();           break;
            case State.LungeTelegraph: UpdateLungeTelegraph(); break;
            case State.Lunge:          UpdateLunge();          break;
            case State.Attack:         UpdateAttack();         break;
            case State.Vulnerable:     UpdateVulnerable();     break;
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
            // Rover a media distancia y embestida lista → salto rápido en vez de caminar.
            if (lungeCooldownTimer <= 0f &&
                distance >= stats.lungeMinRange && distance <= stats.lungeMaxRange)
            {
                BeginLungeTelegraph();
                return;
            }

            // Persecución (en enrage camina ×1,5).
            float speed = stats.moveSpeed * (isEnraged ? stats.enrageSpeedMultiplier : 1f);
            animator.SetBool("IsWalking", true);
            transform.position = Vector2.MoveTowards(
                transform.position, rover.position, speed * Time.deltaTime);
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

    // ── Embestida (telegraph → salto en arco → impacto frente al rover) ────

    private void BeginLungeTelegraph()
    {
        currentState = State.LungeTelegraph;
        stateTimer = 0f;

        animator.SetBool("IsWalking", false);
        if (audioController != null) audioController.PlayAttackSound();   // rugido de aviso

        // El aviso tiene que LEERSE: tinte naranja durante el telegraph.
        if (bodySprite != null && _flashRoutine == null)
            bodySprite.color = lungeTelegraphTint;
    }

    private void UpdateLungeTelegraph()
    {
        if (rover == null) { CancelLunge(); return; }

        FaceRover();   // apunta hasta el último instante…

        stateTimer += Time.deltaTime;
        float telegraph = stats.lungeTelegraphTime / (isEnraged ? stats.enrageSpeedMultiplier : 1f);
        if (stateTimer < telegraph) return;

        // …pero el objetivo se FIJA aquí: el vuelo completo es la ventana de esquiva.
        // Cae DELANTE del rover (del lado del boss), nunca encima: entre ambos siempre
        // hay lungeLandingOffset y el punto queda dentro de la arena por construcción.
        float dir = Mathf.Sign(rover.position.x - transform.position.x);
        if (dir == 0f) dir = 1f;

        _lungeStart = transform.position;
        _lungeTarget = new Vector3(
            rover.position.x - dir * stats.lungeLandingOffset,
            _lungeStart.y,                 // arena plana: despega y aterriza a la misma altura
            _lungeStart.z);

        currentState = State.Lunge;
        stateTimer = 0f;
    }

    private void UpdateLunge()
    {
        float duration = stats.lungeDuration / (isEnraged ? stats.enrageSpeedMultiplier : 1f);
        stateTimer += Time.deltaTime;
        float t = Mathf.Clamp01(stateTimer / duration);

        // Arco parabólico: interpola en X y suma altura 4t(1−t) (máximo en t=0,5).
        Vector3 pos = Vector3.Lerp(_lungeStart, _lungeTarget, t);
        pos.y += stats.lungeArcHeight * 4f * t * (1f - t);
        transform.position = pos;

        if (t >= 1f) LandLunge();
    }

    private void LandLunge()
    {
        transform.position = _lungeTarget;
        lungeCooldownTimer = stats.lungeCooldown / (isEnraged ? stats.enrageSpeedMultiplier : 1f);

        // Restaurar el tinte del telegraph antes de encadenar el combo.
        if (bodySprite != null && _flashRoutine == null)
            bodySprite.color = _baseColor;

        // Onda de impacto: castiga quedarse pegado al punto de caída.
        if (rover != null &&
            Vector2.Distance(transform.position, rover.position) <= stats.lungeImpactRadius)
        {
            var degradation = rover.GetComponentInParent<DegradationSystem>();
            if (degradation != null)
                degradation.TakeDamage(stats.lungeDamage, transform.position);
        }

        FaceRover();

        // Aterriza y encadena directamente el slam de tentáculos → Vulnerable:
        // la embestida siempre termina en una ventana de castigo cerca del jugador.
        attackTimer = 0f;
        BeginAttack();
    }

    /// <summary>Aborta el telegraph (rover perdido) sin dejar el tinte pegado.</summary>
    private void CancelLunge()
    {
        if (bodySprite != null && _flashRoutine == null)
            bodySprite.color = _baseColor;
        currentState = State.Idle;
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

    private void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        // Rango de combo cuerpo a cuerpo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Banda de embestida (min–max)
        Gizmos.color = new Color(1f, 0.55f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, stats.lungeMinRange);
        Gizmos.DrawWireSphere(transform.position, stats.lungeMaxRange);

        // Onda de impacto en el punto de aterrizaje (en juego) o aquí (en editor)
        Gizmos.color = Color.yellow;
        Vector3 impactCenter = Application.isPlaying && currentState == State.Lunge
            ? _lungeTarget
            : transform.position;
        Gizmos.DrawWireSphere(impactCenter, stats.lungeImpactRadius);
    }
}
