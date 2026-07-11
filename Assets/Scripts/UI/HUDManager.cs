using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD mínimo de Level01 — Sprint 03 T2 (GDD §10). Solo PRESENTA datos del
/// <see cref="DegradationSystem"/>; no contiene lógica de juego.
/// Suscribe sus eventos y refleja SI, fase, celdas, daño y muerte.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Sistema (se resuelve por escena si se deja vacío)")]
    [SerializeField] private DegradationSystem _degradation;

    [Header("Barra de Integridad Estructural")]
    [SerializeField] private Slider _siSlider;
    [SerializeField] private Image _siFill;
    [SerializeField] private TMP_Text _siText;
    [SerializeField] private RectTransform _siPulseRoot;   // se escala al recibir daño

    [Header("Celdas solares")]
    [SerializeField] private Image[] _cellSlots;
    [SerializeField] private Color _cellFullColor  = Color.white;
    [SerializeField] private Color _cellEmptyColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("Alert Strip")]
    [SerializeField] private CanvasGroup _alertGroup;
    [SerializeField] private TMP_Text _alertText;

    [Header("Paneles de flujo (los controla GameManager T5)")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _gameOverPanel;

    [Header("Barra de vida del boss (S04 T3 — arte pendiente en HUD_Canvas)")]
    [SerializeField] private GameObject _bossPanel;
    [SerializeField] private Image _bossFill;       // Image tipo Filled
    [SerializeField] private TMP_Text _bossName;

    [Header("Colores de la barra (GDD §10.2)")]
    [SerializeField] private Color _colorHigh     = new Color(0.298f, 0.686f, 0.314f); // #4CAF50  100–61%
    [SerializeField] private Color _colorMid      = new Color(1f,     0.655f, 0.149f); // #FFA726   60–41%
    [SerializeField] private Color _colorLow      = new Color(0.957f, 0.263f, 0.212f); // #F44336   40–21%
    [SerializeField] private Color _colorCritical = new Color(0.545f, 0f,     0f);     // #8B0000   20–1%

    [Header("Pulso de daño")]
    [SerializeField] private float _pulseScale = 1.08f;
    [SerializeField] private float _pulseTime = 0.12f;

    private Coroutine _pulseRoutine;
    private Coroutine _blinkRoutine;
    private Coroutine _alertRoutine;
    private float _blinkInterval = -1f; // -1 = sin inicializar
    private bool _subscribed;

    private void Awake()
    {
        if (_degradation == null)
            _degradation = FindFirstObjectByType<DegradationSystem>();

        if (_alertGroup != null) _alertGroup.alpha = 0f;
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_bossPanel != null) _bossPanel.SetActive(false);
    }

    private void OnEnable()  => Subscribe();
    private void OnDisable() => Unsubscribe();

    /// <summary>
    /// Re-vincula el HUD a un DegradationSystem concreto y re-sincroniza la barra.
    /// La llama GameManager en cada carga de escena (tras aplicar la herencia de SI)
    /// para garantizar que HUD y GameManager miran a la MISMA instancia.
    /// </summary>
    public void Bind(DegradationSystem degradation)
    {
        if (degradation != null && degradation != _degradation)
        {
            Unsubscribe();
            _degradation = degradation;
        }
        Subscribe();
        SyncNow();
    }

    private void Subscribe()
    {
        if (_subscribed) return;
        if (_degradation == null)
        {
            // Interludio isométrico (S06 T1) u otra escena sin supervivencia:
            // el HUD queda solo como contenedor de pausa/Game Over.
            Debug.Log("[HUDManager] Sin DegradationSystem en la escena: se ocultan barra de SI " +
                      "y celdas (esperado en el interludio; en un nivel es un bug).");
            SetSurvivalWidgetsActive(false);
            return;
        }
        SetSurvivalWidgetsActive(true);

        _degradation.OnSIChanged      += UpdateSIBar;
        _degradation.OnPhaseChanged   += UpdatePhaseEffects;
        _degradation.OnCellsChanged   += UpdateCells;
        _degradation.OnDamageReceived += HandleDamage;
        // Si hay GameManager (T5) él gobierna la muerte→Game Over; si no, el HUD lo muestra solo.
        if (GameManager.Instance == null) _degradation.OnDeath += ShowGameOver;
        _subscribed = true;

        SyncNow();
    }

    private void Unsubscribe()
    {
        if (!_subscribed || _degradation == null) { _subscribed = false; return; }

        _degradation.OnSIChanged      -= UpdateSIBar;
        _degradation.OnPhaseChanged   -= UpdatePhaseEffects;
        _degradation.OnCellsChanged   -= UpdateCells;
        _degradation.OnDamageReceived -= HandleDamage;
        if (GameManager.Instance == null) _degradation.OnDeath -= ShowGameOver;
        _subscribed = false;
    }

    /// <summary>
    /// Muestra/oculta los widgets de supervivencia (barra de SI y celdas). En el
    /// interludio isométrico no hay DegradationSystem y mostrarlos congelados
    /// sería mentirle al jugador (GDD: el interludio no tiene daño ni muerte).
    /// </summary>
    private void SetSurvivalWidgetsActive(bool active)
    {
        if (_siPulseRoot != null) _siPulseRoot.gameObject.SetActive(active);
        if (_siSlider != null) _siSlider.gameObject.SetActive(active);
        if (_cellSlots != null)
            foreach (var slot in _cellSlots)
                if (slot != null && slot.transform.parent != null)
                    slot.transform.parent.gameObject.SetActive(active);
    }

    /// <summary>Refresca barra y celdas con el estado actual (no espera al próximo evento).</summary>
    private void SyncNow()
    {
        if (_degradation == null) return;
        UpdateSIBar(_degradation.CurrentSI);
        UpdateCells(_degradation.CellsInReserve);
    }

    // ── Barra de SI ───────────────────────────────────────────────────────
    private void UpdateSIBar(float si)
    {
        float max = _degradation != null ? _degradation.MaxSI : 100f;
        float pct = max > 0f ? si / max : 0f;

        if (_siSlider != null) _siSlider.value = pct;
        if (_siText != null)   _siText.text = $"SI {Mathf.CeilToInt(si)}%";
        if (_siFill != null)
        {
            // Soporta tanto Image tipo Filled (barra directa) como fill de un Slider.
            if (_siFill.type == Image.Type.Filled) _siFill.fillAmount = pct;
            _siFill.color = ColorForPercent(pct);
        }

        // Parpadeo del texto según umbral (GDD §10.2): lento <40%, rápido <20%.
        float interval = pct <= 0.20f ? 0.18f : (pct <= 0.40f ? 0.40f : 0f);
        SetBlink(interval);
    }

    private Color ColorForPercent(float pct)
    {
        if (pct > 0.60f) return _colorHigh;
        if (pct > 0.40f) return _colorMid;
        if (pct > 0.20f) return _colorLow;
        return _colorCritical;
    }

    private void UpdatePhaseEffects(int phase)
    {
        // Hook para efectos por fase (vignette, glitch…). MVP: refresca color/parpadeo.
        if (_degradation != null) UpdateSIBar(_degradation.CurrentSI);
    }

    // ── Celdas ──────────────────────────────────────────────────────────────
    private void UpdateCells(int count)
    {
        if (_cellSlots == null) return;
        for (int i = 0; i < _cellSlots.Length; i++)
        {
            if (_cellSlots[i] == null) continue;
            _cellSlots[i].color = i < count ? _cellFullColor : _cellEmptyColor;
        }
    }

    // ── Pulso de daño ────────────────────────────────────────────────────────
    private void HandleDamage(float amount)
    {
        if (_siPulseRoot == null || _pulseRoutine != null) return; // el contacto daña cada frame
        _pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 baseScale = Vector3.one;
        Vector3 peak = Vector3.one * _pulseScale;
        float half = Mathf.Max(0.01f, _pulseTime * 0.5f);

        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            _siPulseRoot.localScale = Vector3.Lerp(baseScale, peak, t / half);
            yield return null;
        }
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            _siPulseRoot.localScale = Vector3.Lerp(peak, baseScale, t / half);
            yield return null;
        }
        _siPulseRoot.localScale = baseScale;
        _pulseRoutine = null;
    }

    // ── Parpadeo del texto de SI ─────────────────────────────────────────────
    private void SetBlink(float interval)
    {
        if (Mathf.Approximately(interval, _blinkInterval)) return;
        _blinkInterval = interval;

        if (_blinkRoutine != null) { StopCoroutine(_blinkRoutine); _blinkRoutine = null; }
        if (_siText != null) SetTextAlpha(_siText, 1f);

        if (interval > 0f) _blinkRoutine = StartCoroutine(BlinkRoutine(interval));
    }

    private IEnumerator BlinkRoutine(float interval)
    {
        bool on = true;
        var wait = new WaitForSecondsRealtime(interval);
        while (true)
        {
            if (_siText != null) SetTextAlpha(_siText, on ? 1f : 0.25f);
            on = !on;
            yield return wait;
        }
    }

    private static void SetTextAlpha(TMP_Text t, float a)
    {
        Color c = t.color; c.a = a; t.color = c;
    }

    // ── Alert Strip (lo usa T4: "CHECKPOINT REGISTRADO") ─────────────────────
    public void ShowAlert(string text, float duration)
    {
        if (_alertGroup == null || _alertText == null) return;
        _alertText.text = (text ?? string.Empty).ToUpperInvariant();
        if (_alertRoutine != null) StopCoroutine(_alertRoutine);
        _alertRoutine = StartCoroutine(AlertRoutine(duration));
    }

    private IEnumerator AlertRoutine(float duration)
    {
        _alertGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration));

        const float fade = 0.5f;
        for (float t = 0; t < fade; t += Time.unscaledDeltaTime)
        {
            _alertGroup.alpha = 1f - (t / fade);
            yield return null;
        }
        _alertGroup.alpha = 0f;
        _alertRoutine = null;
    }

    // ── Paneles de flujo (los activa GameManager T5; públicos para él) ───────
    public void ShowGameOver()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
    }

    public void ShowPause(bool show)
    {
        if (_pausePanel != null) _pausePanel.SetActive(show);
    }

    public void HideGameOver()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
    }

    // ── Barra de vida del boss (S04 T3 — la maneja LeviatanAI) ───────────────
    // Null-safe: si el arte aún no está en HUD_Canvas, simplemente no se muestra.
    public void ShowBossBar(string bossName)
    {
        if (_bossName != null) _bossName.text = (bossName ?? string.Empty).ToUpperInvariant();
        if (_bossPanel != null) _bossPanel.SetActive(true);
    }

    public void UpdateBossBar(float normalized)
    {
        if (_bossFill == null) return;
        normalized = Mathf.Clamp01(normalized);
        if (_bossFill.type == Image.Type.Filled) _bossFill.fillAmount = normalized;
        else _bossFill.rectTransform.localScale = new Vector3(normalized, 1f, 1f);
    }

    public void HideBossBar()
    {
        if (_bossPanel != null) _bossPanel.SetActive(false);
    }
}
