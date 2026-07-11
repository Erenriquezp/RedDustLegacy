using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Terminal embebido del HUD para las fichas de escaneo (S05 T1, GDD §11.1).
/// Typewriter ~40 car/s, colores por severidad (blanco/ambar/rojo) y corrupción
/// DETERMINISTA del texto con SI ≤29% (Fase ≥4) — nunca corrompe el encabezado
/// ni la primera oración. Crear con Tools → Red Dust → Scaffold Scan Terminal.
/// Guía: Docs/Architecture/ScanSystem.md.
/// </summary>
public class ScanTerminalUI : MonoBehaviour
{
    [Header("Referencias (las cablea el scaffolder)")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;
    [Tooltip("Línea de acento superior: toma el color de severidad.")]
    [SerializeField] private Image accent;

    [Header("Presentación (GDD §11.1)")]
    [SerializeField] private float charsPerSecond = 40f;
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private Color colorNominal = Color.white;
    [SerializeField] private Color colorAdvertencia = new Color(1f, 0.72f, 0.2f);
    [SerializeField] private Color colorAnomalia = new Color(0.96f, 0.3f, 0.26f);

    [Header("Corrupción por SI (GDD §11.1)")]
    [Tooltip("Cadencia del parpadeo entre las dos variantes corruptas.")]
    [SerializeField] private float flickerInterval = 0.25f;

    private static readonly char[] Glyphs = { '▓', '█', '░', '#', '@', '/' };

    private Coroutine _routine;

    private void Awake()
    {
        if (group != null) group.alpha = 0f;
    }

    /// <summary>Muestra la ficha. phase = fase de degradación actual (corrupción con fase ≥4).</summary>
    public void Show(ScanDataSO data, int phase)
    {
        if (data == null || group == null) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(data, phase));
    }

    /// <summary>Fade de salida 0,3 s (GDD §11.1). La llama ScanSystem al soltar Scan.</summary>
    public void Hide()
    {
        if (group == null || group.alpha <= 0f) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeOutRoutine());
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator ShowRoutine(ScanDataSO data, int phase)
    {
        Color c = ColorFor(data.severidad);
        if (headerText != null)
        {
            headerText.color = c;
            headerText.text = $"{data.EncabezadoTexto} — {data.id}";
        }
        if (accent != null) accent.color = c;
        if (bodyText != null) bodyText.color = c;

        group.alpha = 1f;

        // Dos variantes corruptas fijas (semilla por id): el texto se corrompe
        // IGUAL en cada lectura y el parpadeo alterna solo entre ellas — nunca
        // aleatorio por frame (GDD §11.1 + guía).
        float severity = SeverityFor(phase);
        int seed = data.id != null ? data.id.GetHashCode() : 0;
        string v1 = Corrupt(data.texto, severity, seed);
        string v2 = Corrupt(data.texto, severity, seed + 1);

        if (bodyText == null) yield break;

        bodyText.text = v1;
        bodyText.maxVisibleCharacters = 0;
        int total = v1.Length;

        // Typewriter (~40 car/s) con parpadeo de corrupción a 4 Hz.
        float shown = 0f, flick = 0f;
        bool alt = false;

        while ((int)shown < total)
        {
            shown += charsPerSecond * Time.unscaledDeltaTime;
            flick += Time.unscaledDeltaTime;

            if (severity > 0f && flick >= flickerInterval)
            {
                flick = 0f;
                alt = !alt;
                bodyText.text = alt ? v2 : v1;
            }

            bodyText.maxVisibleCharacters = Mathf.Min(total, (int)shown);
            yield return null;
        }

        bodyText.maxVisibleCharacters = int.MaxValue;

        // Texto completo: cursor parpadeante + parpadeo de corrupción sostenido.
        while (true)
        {
            yield return new WaitForSecondsRealtime(flickerInterval);
            if (severity > 0f) alt = !alt;
            string cursor = (Time.unscaledTime % 1f) < 0.5f ? " ▌" : "";
            bodyText.text = (alt ? v2 : v1) + cursor;
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        float start = group.alpha;
        for (float t = 0f; t < fadeOutTime; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, 0f, t / fadeOutTime);
            yield return null;
        }
        group.alpha = 0f;
        _routine = null;
    }

    private Color ColorFor(ScanDataSO.Severidad s) => s switch
    {
        ScanDataSO.Severidad.Advertencia => colorAdvertencia,
        ScanDataSO.Severidad.Anomalia    => colorAnomalia,
        _                                => colorNominal,
    };

    /// <summary>Severidad de corrupción por fase de degradación (GDD §11.1: desde SI 29% = Fase 4).</summary>
    private static float SeverityFor(int phase) => phase switch
    {
        >= 6 => 0.40f,
        5    => 0.25f,
        4    => 0.10f,
        _    => 0f,
    };

    /// <summary>
    /// Corrupción determinista: misma semilla → mismos glifos. Protege la primera
    /// oración completa y los espacios — siempre se entiende QUÉ se está leyendo.
    /// </summary>
    private static string Corrupt(string s, float severity, int seed)
    {
        if (severity <= 0f || string.IsNullOrEmpty(s)) return s ?? string.Empty;

        var rng = new System.Random(seed);
        char[] chars = s.ToCharArray();
        int protectedUpTo = s.IndexOf('.') + 1;   // primera oración intacta

        for (int i = 0; i < chars.Length; i++)
        {
            bool corrupt = rng.NextDouble() < severity;   // consumir SIEMPRE (determinismo)
            if (i < protectedUpTo || char.IsWhiteSpace(chars[i])) continue;
            if (corrupt) chars[i] = Glyphs[rng.Next(Glyphs.Length)];
        }

        return new string(chars);
    }
}
