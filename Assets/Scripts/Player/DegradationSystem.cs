using System;
using UnityEngine;

/// <summary>
/// Sistema de Integridad Estructural (SI) del rover — Sprint 03 T1 (GDD §4).
/// Vive en el mismo GameObject que <see cref="PlayerController"/>. Mantiene la SI,
/// calcula la fase de degradación (1–6) y aplica sus modificadores sobre una COPIA
/// runtime del <see cref="RoverStatsSO"/> (nunca toca el .asset original).
/// Es la autoridad de daño: los enemigos llaman a <see cref="TakeDamage"/>.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class DegradationSystem : MonoBehaviour
{
    [Header("Integridad Estructural (GDD §4.1)")]
    [SerializeField] private float maxSI = 100f;
    [SerializeField] private float startSI = 74f;          // Fase 1 (NOMINAL)
    [SerializeField] private float deathThreshold = 1f;    // muerte si SI < 1

    [Header("Celdas solares (GDD §4.1)")]
    [SerializeField] private int startingCells = 0;
    [SerializeField] private int maxCells = 2;
    [SerializeField] private float cellRestoreAmount = 20f;

    [Header("Daño por caída (GDD §4.3)")]
    [Tooltip("Descenso neto (u) mínimo para dañar en Fase 1.")]
    [SerializeField] private float fallMinPhase1 = 3f;
    [Tooltip("Descenso neto (u) mínimo para dañar en Fase ≥2.")]
    [SerializeField] private float fallMinPhase2Plus = 2f;
    [Tooltip("Descenso neto (u) a partir del cual el daño es máximo.")]
    [SerializeField] private float fallHeavyDistance = 6f;
    [SerializeField] private float fallLightDamage = 10f;  // 3–5 u
    [SerializeField] private float fallHeavyDamage = 25f;  // ≥6 u (cap)

    [Header("Invulnerabilidad (i-frames, GDD §4.3)")]
    [Tooltip("Segundos de invulnerabilidad tras recibir daño. Corta el daño en cascada.")]
    [SerializeField] private float invulnDuration = 0.6f;

    // ── Eventos (los consumen HUD / GameManager — Sprint 03 T2/T5) ──────────
    public event Action<float> OnSIChanged;      // SI actual tras el cambio
    public event Action<int>   OnPhaseChanged;   // fase 1–6
    public event Action<float> OnDamageReceived; // cantidad de daño aplicada
    public event Action        OnDeath;          // SI < deathThreshold (una vez)
    public event Action<int>   OnCellsChanged;   // celdas en reserva (0–maxCells)

    // ── API pública ─────────────────────────────────────────────────────────
    public float CurrentSI      => _currentSI;
    public float MaxSI          => maxSI;
    public int   CurrentPhase   => _currentPhase;
    public int   CellsInReserve => _cells;
    public bool  IsDead         => _isDead;

    // ── Estado interno ───────────────────────────────────────────────────────
    private PlayerController _controller;
    private RoverStatsSO _runtime;   // copia que el controller lee de verdad
    private float _currentSI;
    private int   _currentPhase = 1;
    private int   _cells;
    private bool  _isDead;
    private float _invulnUntil;   // Time.time hasta el que el rover es invulnerable (i-frames)

    // Valores base de los campos modificables (para reaplicar sin acumular error).
    private float _baseMaxRunSpeed, _baseGroundAccel, _baseJumpBuffer;
    private float _baseDashCooldown, _baseJumpForce, _baseCoyote;

    // Tracking de caída (descenso neto por debajo del punto de despegue).
    private float _takeoffY;
    private bool  _hasTakeoff;

    // ═══════════════════════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();

        // Copia runtime del SO: el controller pasa a leer de ella; el .asset queda intacto.
        RoverStatsSO baseStats = _controller.GetStats();
        _runtime = Instantiate(baseStats);
        _runtime.name = baseStats.name + " (Runtime)";
        _controller.SetStats(_runtime);

        // Snapshot de los campos modificables (la copia parte de los valores base).
        _baseMaxRunSpeed  = _runtime.maxRunSpeed;
        _baseGroundAccel  = _runtime.groundAcceleration;
        _baseJumpBuffer   = _runtime.jumpBufferTime;
        _baseDashCooldown = _runtime.dashCooldown;
        _baseJumpForce    = _runtime.jumpForce;
        _baseCoyote       = _runtime.coyoteTime;

        _currentSI    = Mathf.Clamp(startSI, 0f, maxSI);
        _cells        = Mathf.Clamp(startingCells, 0, maxCells);
        _currentPhase = CalculatePhase(_currentSI);
    }

    private void OnEnable()  => _controller.OnGroundedChanged += HandleGroundedChanged;
    private void OnDisable() => _controller.OnGroundedChanged -= HandleGroundedChanged;

    private void Start()
    {
        // Aplica la fase inicial y publica el estado de arranque (para el HUD).
        ApplyPhaseModifiers(_currentPhase);
        OnPhaseChanged?.Invoke(_currentPhase);
        OnSIChanged?.Invoke(_currentSI);
        OnCellsChanged?.Invoke(_cells);
    }

    private void OnDestroy()
    {
        // Evita fugas de la copia runtime entre recargas de escena.
        if (_runtime != null) Destroy(_runtime);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Daño y curación

    /// <summary>Daño sin fuente (caídas, etc.): no genera knockback horizontal.</summary>
    public void TakeDamage(float amount) => TakeDamage(amount, null);

    /// <summary>
    /// Aplica daño con i-frames: durante <see cref="invulnDuration"/> tras un golpe se ignora
    /// el daño nuevo (corta la cascada de pinchos/enemigos). Si se pasa la posición de la fuente,
    /// el rover retrocede un poco para notar el impacto (GDD §4.3).
    /// </summary>
    public void TakeDamage(float amount, Vector2? sourcePosition)
    {
        if (_isDead || amount <= 0f) return;
        if (Time.time < _invulnUntil) return;          // i-frames: corta el daño en cascada
        _invulnUntil = Time.time + invulnDuration;

        _currentSI = Mathf.Max(0f, _currentSI - amount);

        OnDamageReceived?.Invoke(amount);
        _controller.NotifyDamageReceived(amount);
        OnSIChanged?.Invoke(_currentSI);

        if (sourcePosition.HasValue)
            _controller.ApplyKnockback(sourcePosition.Value);

        RecalculatePhase();

        if (_currentSI < deathThreshold)
            Die();
    }

    public void UseSolarCell()
    {
        if (_isDead || _cells <= 0 || _currentSI >= maxSI) return;

        _cells--;
        _currentSI = Mathf.Min(maxSI, _currentSI + cellRestoreAmount);
        OnSIChanged?.Invoke(_currentSI);
        OnCellsChanged?.Invoke(_cells);
        RecalculatePhase();
    }

    /// <summary>Recoge una celda (pickup). Devuelve false si la reserva está llena.</summary>
    public bool AddCell()
    {
        if (_cells >= maxCells) return false;
        _cells++;
        OnCellsChanged?.Invoke(_cells);
        return true;
    }

    /// <summary>Fija la SI directamente (respawn desde checkpoint — Sprint 03 T4).</summary>
    public void SetSI(float value)
    {
        bool wasDead = _isDead;
        _isDead = false;
        _currentSI = Mathf.Clamp(value, 0f, maxSI);
        OnSIChanged?.Invoke(_currentSI);
        RecalculatePhase();
        if (wasDead) _controller.NotifyRevive();   // reactiva animator/audio tras la muerte
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;
        OnDeath?.Invoke();
        _controller.NotifyDeath();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Fases

    private void RecalculatePhase()
    {
        int newPhase = CalculatePhase(_currentSI);
        if (newPhase == _currentPhase) return;

        _currentPhase = newPhase;
        ApplyPhaseModifiers(newPhase);
        OnPhaseChanged?.Invoke(newPhase);
    }

    private int CalculatePhase(float si)
    {
        if (si >= 74f) return 1; // NOMINAL     100–74
        if (si >= 61f) return 2; // DESGASTE     73–61
        if (si >= 47f) return 3; // AVERÍA       60–47
        if (si >= 29f) return 4; // CRÍTICO      46–29
        if (si >= 9f)  return 5; // EMERGENCIA   28–9
        return 6;                // EXTINCIÓN     8–1
    }

    /// <summary>
    /// Reaplica desde los valores base los modificadores ACUMULATIVOS hasta `phase`
    /// (GDD §4.2). Resetea primero para no acumular error entre transiciones.
    /// </summary>
    private void ApplyPhaseModifiers(int phase)
    {
        // 1) Restaurar valores base y toggles.
        _runtime.maxRunSpeed        = _baseMaxRunSpeed;
        _runtime.groundAcceleration = _baseGroundAccel;
        _runtime.jumpBufferTime     = _baseJumpBuffer;
        _runtime.dashCooldown       = _baseDashCooldown;
        _runtime.jumpForce          = _baseJumpForce;
        _runtime.coyoteTime         = _baseCoyote;
        _controller.AerialDashEnabled = true;
        _controller.WallJumpEnabled   = true;
        _controller.DashEnabled       = true;

        // 2) Aplicar modificadores acumulados hasta la fase actual.
        if (phase >= 3) // AVERÍA
        {
            _runtime.dashCooldown *= 1.5f;
            _controller.AerialDashEnabled = false;
        }
        if (phase >= 4) // CRÍTICO
        {
            _runtime.maxRunSpeed        *= 0.75f;
            _runtime.groundAcceleration *= 0.80f;
            _runtime.jumpBufferTime      = 0.06f;
        }
        if (phase >= 5) // EMERGENCIA
        {
            _runtime.jumpForce  *= 0.85f;
            _runtime.coyoteTime  = 0.06f;
            _controller.WallJumpEnabled = false;
        }
        if (phase >= 6) // EXTINCIÓN
        {
            _runtime.maxRunSpeed *= 0.50f;  // acumulativo sobre el ×0.75 de Fase 4
            _controller.DashEnabled = false;
        }
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════════════
    #region Daño por caída

    private void HandleGroundedChanged(bool isGrounded)
    {
        if (!isGrounded)
        {
            // Despega: registra la altura de salida (no penaliza saltos: medimos descenso NETO).
            _takeoffY = transform.position.y;
            _hasTakeoff = true;
        }
        else if (_hasTakeoff)
        {
            // Aterriza: el descenso por debajo del punto de salida define el daño.
            float netDescent = _takeoffY - transform.position.y;
            _hasTakeoff = false;
            ApplyFallDamage(netDescent);
        }
    }

    private void ApplyFallDamage(float netDescent)
    {
        if (_isDead) return;

        float threshold = _currentPhase >= 2 ? fallMinPhase2Plus : fallMinPhase1;
        if (netDescent < threshold) return;

        float damage = netDescent >= fallHeavyDistance ? fallHeavyDamage : fallLightDamage;
        TakeDamage(damage);
    }

    #endregion
}
