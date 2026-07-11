using System;
using UnityEngine;

/// <summary>
/// Sistema de escaneo del rover (S05 T1, GDD §11). Vive en el Player junto a
/// PlayerController/DegradationSystem. Mientras se mantiene Scan: busca el
/// Scannable más cercano dentro del radio (capa Interactable), lo "adquiere"
/// tras acquireTime y muestra su ficha en el ScanTerminalUI del HUD. Dibuja un
/// anillo de alcance procedimental (LineRenderer, sin assets) y resalta el
/// objetivo. El radio pasa de 3 a 5 u con el upgrade Escaneo Mejorado (T5, vía
/// ScanRadius). Guía: Docs/Architecture/ScanSystem.md.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class ScanSystem : MonoBehaviour
{
    [Header("Detección (GDD §11.1)")]
    [Tooltip("Radio base 3 u; el upgrade Escaneo Mejorado (T5) lo sube a 5 vía ScanRadius.")]
    [SerializeField] private float scanRadius = 3f;
    [Tooltip("Segundos que el objetivo debe sostenerse bajo el radio antes de mostrar la ficha.")]
    [SerializeField] private float acquireTime = 0.4f;
    [Tooltip("Capas escaneables. Vacío = Interactable.")]
    [SerializeField] private LayerMask interactableMask;

    [Header("UI (se resuelve por escena si se deja vacío)")]
    [SerializeField] private ScanTerminalUI terminal;

    [Header("Feedback visual (procedimental, sin assets)")]
    [SerializeField] private Color ringColor = new Color(0.3f, 0.9f, 1f, 0.65f);
    [SerializeField] private Color highlightColor = new Color(0.6f, 1f, 1f, 1f);
    [Tooltip("Segundos que tarda el anillo en expandirse del centro al radio.")]
    [SerializeField] private float ringPeriod = 1.1f;

    /// <summary>Escaneo completado (primera vez): lo consumen códex (S06) y CinematicManager (T4).</summary>
    public event Action<ScanDataSO> OnScanCompleted;

    /// <summary>Radio actual. El UpgradeManager (T5) lo sube a 5 con Escaneo Mejorado.</summary>
    public float ScanRadius { get => scanRadius; set => scanRadius = value; }

    private const int RingSegments = 48;
    private const float PollInterval = 0.1f;   // 10 Hz — no cada frame

    private PlayerController _controller;
    private DegradationSystem _degradation;
    private HUDManager _hud;

    private bool _scanning;
    private bool _warnedNoScannable;   // un aviso por pulsación, no 10 Hz de spam
    private float _pollTimer;
    private float _acquireTimer;
    private float _ringPhase;

    private Scannable _target;        // candidato bajo el radio
    private Scannable _shownTarget;   // ficha ya mostrada (no re-mostrar cada frame)

    private LineRenderer _ring;
    private SpriteRenderer _targetSprite;
    private Color _targetBaseColor;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _degradation = GetComponent<DegradationSystem>();

        if (interactableMask == 0)
            interactableMask = LayerMask.GetMask("Interactable");

        BuildRing();
    }

    private void Start()
    {
        if (terminal == null)
            terminal = FindFirstObjectByType<ScanTerminalUI>(FindObjectsInactive.Include);
        if (terminal == null)
            Debug.LogWarning("[ScanSystem] Esta escena NO tiene ScanTerminalUI: las fichas no se " +
                             "mostrarán. Ejecuta Tools → Red Dust → Scaffold Scan Terminal DENTRO " +
                             "del prefab HUD_Canvas y guárdalo.", this);
        _hud = FindFirstObjectByType<HUDManager>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        _controller.OnScanStarted += HandleScanStarted;
        _controller.OnScanStopped += HandleScanStopped;
    }

    private void OnDisable()
    {
        _controller.OnScanStarted -= HandleScanStarted;
        _controller.OnScanStopped -= HandleScanStopped;
        HandleScanStopped();   // no dejar anillo/ficha colgados si se desactiva el Player
    }

    private void HandleScanStarted()
    {
        _scanning = true;
        _warnedNoScannable = false;
        _ringPhase = 0f;
        _pollTimer = PollInterval;   // primer sondeo inmediato
        if (_ring != null) _ring.enabled = true;
    }

    private void HandleScanStopped()
    {
        _scanning = false;
        ClearTarget();
        _shownTarget = null;
        if (_ring != null) _ring.enabled = false;
        if (terminal != null) terminal.Hide();   // fade 0,3 s (GDD §11.1)
    }

    private void Update()
    {
        if (!_scanning) return;

        UpdateRing();

        // Sondeo del objetivo a 10 Hz.
        _pollTimer += Time.deltaTime;
        if (_pollTimer >= PollInterval)
        {
            _pollTimer = 0f;
            RefreshTarget();
        }

        if (_target == null) return;

        PulseTargetHighlight();

        // Adquisición: el objetivo debe sostenerse acquireTime bajo el radio.
        _acquireTimer += Time.deltaTime;
        if (_acquireTimer >= acquireTime && _shownTarget != _target)
            CompleteScan(_target);
    }

    // ── Detección ───────────────────────────────────────────────────────────

    private void RefreshTarget()
    {
        Scannable best = null;
        float bestDist = float.MaxValue;

        var hits = Physics2D.OverlapCircleAll(transform.position, scanRadius, interactableMask);
        foreach (var hit in hits)
        {
            var scannable = hit.GetComponentInParent<Scannable>();
            if (scannable == null || scannable.data == null) continue;

            float dist = Vector2.Distance(transform.position, scannable.transform.position);
            if (dist < bestDist) { bestDist = dist; best = scannable; }
        }

        // Diagnóstico: hay colliders en capa Interactable pero ninguno es escaneable.
        if (best == null && hits.Length > 0 && !_warnedNoScannable)
        {
            _warnedNoScannable = true;
            Debug.LogWarning($"[ScanSystem] {hits.Length} collider(s) en capa Interactable dentro " +
                             "del radio, pero sin componente Scannable con data asignada.", this);
        }

        if (best != _target)
        {
            ClearTarget();
            _target = best;
            _acquireTimer = 0f;

            if (_target != null)
            {
                _targetSprite = _target.GetComponentInChildren<SpriteRenderer>();
                if (_targetSprite != null) _targetBaseColor = _targetSprite.color;
            }
        }
    }

    private void ClearTarget()
    {
        if (_targetSprite != null) _targetSprite.color = _targetBaseColor;
        _targetSprite = null;
        _target = null;
        _acquireTimer = 0f;
    }

    private void CompleteScan(Scannable scannable)
    {
        _shownTarget = scannable;
        Debug.Log($"[ScanSystem] Escaneo completado: {scannable.data.id} ({scannable.name})", scannable);

        bool firstTime = scannable.Consume();
        int phase = _degradation != null ? _degradation.CurrentPhase : 1;

        if (terminal != null)
            terminal.Show(scannable.data, phase);

        if (!firstTime) return;

        if (_hud != null)
            _hud.ShowAlert($"NUEVO REGISTRO — {scannable.data.id}", 2f);

        OnScanCompleted?.Invoke(scannable.data);

        // Handoff a T4: cuando exista CinematicManager, disparar aquí
        // PlayFlashback(data.flashbackId) — él valida el SI mínimo (GDD §11.3).
        if (!string.IsNullOrEmpty(scannable.data.flashbackId))
            Debug.Log($"[ScanSystem] Flashback pendiente de T4: {scannable.data.flashbackId}");
    }

    // ── Feedback visual ──────────────────────────────────────────────────────

    private void BuildRing()
    {
        var go = new GameObject("ScanRing");
        go.transform.SetParent(transform, false);

        _ring = go.AddComponent<LineRenderer>();
        _ring.useWorldSpace = false;
        _ring.loop = true;
        _ring.positionCount = RingSegments;
        _ring.widthMultiplier = 0.05f;
        // URP: preferir el shader 2D unlit; "Sprites/Default" queda como fallback.
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null) _ring.material = new Material(shader);
        _ring.sortingOrder = 20;
        _ring.enabled = false;
    }

    private void UpdateRing()
    {
        if (_ring == null) return;

        // Pulso que se expande del centro al radio real: el jugador VE el alcance.
        _ringPhase += Time.deltaTime / Mathf.Max(0.1f, ringPeriod);
        if (_ringPhase > 1f) _ringPhase -= 1f;

        float r = Mathf.Lerp(0.3f, scanRadius, _ringPhase);
        Color c = ringColor;
        c.a = ringColor.a * (1f - _ringPhase);
        _ring.startColor = _ring.endColor = c;

        for (int i = 0; i < RingSegments; i++)
        {
            float a = i * Mathf.PI * 2f / RingSegments;
            _ring.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
    }

    private void PulseTargetHighlight()
    {
        if (_targetSprite == null) return;
        float k = Mathf.PingPong(Time.time * 3f, 1f);
        _targetSprite.color = Color.Lerp(_targetBaseColor, highlightColor, k);
    }

    private void OnDestroy()
    {
        if (_ring != null && _ring.material != null)
            Destroy(_ring.material);   // material procedimental: sin fugas
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
