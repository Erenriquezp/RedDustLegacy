using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de hints tutoriales contextuales — se muestran UNA sola vez en toda
/// la vida del jugador (PlayerPrefs). Se autoconfigura en cada escena de juego
/// sin necesidad de colocarlo manualmente: lo instancia RuntimeInitializeOnLoadMethod.
///
/// HINTS GESTIONADOS:
///   HINT_DASH       → Al cargar Level01 por primera vez (Shift = Dash).
///   HINT_JUMP       → La primera vez que el Player salta (Espacio = Saltar).
///   HINT_SCAN       → Al entrar al rango de un Scannable por primera vez (E = Escanear).
///   HINT_CELL_USE   → La primera vez que el Player recoge una celda (Q = Usar celda).
///
/// Nota: HINT_CELL_USE reemplaza el ShowAlert directo de EnergyCellPickup para
/// que no se repita en cada pickup posterior.
/// </summary>
public class TutorialHintSystem : MonoBehaviour
{
    // ── Claves PlayerPrefs ────────────────────────────────────────────────
    private const string KEY_DASH     = "HINT_DASH";
    private const string KEY_JUMP     = "HINT_JUMP";
    private const string KEY_SCAN     = "HINT_SCAN";
    private const string KEY_CELL_USE = "HINT_CELL_USE";

    // ── Config de tiempos ─────────────────────────────────────────────────
    /// <summary>Pausa en segundos antes de mostrar el hint de inicio de nivel.</summary>
    private const float LevelStartDelay      = 2.5f;
    /// <summary>Duracion en pantalla de cada hint.</summary>
    private const float HintDuration         = 3.5f;
    /// <summary>Rango (unidades) al que un Scannable dispara el hint de escaneo.</summary>
    private const float ScanProximityRange   = 3.5f;
    /// <summary>Intervalo de sondeo para la deteccion de proximidad (segundos).</summary>
    private const float ProximityCheckInterval = 0.4f;

    // ── Internos ──────────────────────────────────────────────────────────
    private HUDManager      _hud;
    private PlayerController _player;
    private bool _jumpSubscribed;
    private Coroutine _proximityRoutine;

    // ── Bootstrap: instancia automatica antes de cargar la primera escena ─
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("[TutorialHintSystem]");
        go.AddComponent<TutorialHintSystem>();
        DontDestroyOnLoad(go);
    }

    // ── Ciclo de vida ─────────────────────────────────────────────────────
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribePlayer();
    }

    // ── Entrada por escena ────────────────────────────────────────────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        UnsubscribePlayer();
        _hud    = null;
        _player = null;

        bool isLevel01 = scene.name == SceneLoader.Level01Scene;
        bool isLevel02 = scene.name == SceneLoader.Level02Scene;
        if (!isLevel01 && !isLevel02) return;

        // Resolucion diferida: los singletons y el HUD se crean en Awake;
        // si buscamos en el mismo frame podriamos no encontrarlos aun.
        StartCoroutine(InitDelayed(isLevel01));
    }

    private IEnumerator InitDelayed(bool isLevel01)
    {
        // Esperar un frame para que todos los Awake de la escena terminen.
        yield return null;

        _hud    = FindFirstObjectByType<HUDManager>(FindObjectsInactive.Include);
        _player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);

        // Hint de Dash: solo en Level01 y solo si nunca se ha visto.
        if (isLevel01 && !Seen(KEY_DASH))
            StartCoroutine(ShowLevelStartHint());

        // Hint de Salto: en cualquier nivel de gameplay, tras el primer salto.
        if (!Seen(KEY_JUMP) && _player != null)
        {
            _player.OnJumped += OnFirstJump;
            _jumpSubscribed = true;
        }

        // Hint de Escaneo: polling de proximidad mientras no se haya visto.
        if (!Seen(KEY_SCAN))
            _proximityRoutine = StartCoroutine(ScanProximityLoop());
    }

    // ── Hint 1: DASH (inicio de Level01) ──────────────────────────────────
    private IEnumerator ShowLevelStartHint()
    {
        yield return new WaitForSeconds(LevelStartDelay);

        // No mostrar si el juego ya no esta en Playing (murio, pauso, etc.)
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            yield break;

        ShowHint(KEY_DASH, "SHIFT — DASH  |  W — SALTAR");
    }

    // ── Hint 2: SALTO (primer salto del jugador) ───────────────────────────
    private void OnFirstJump()
    {
        if (_player != null) _player.OnJumped -= OnFirstJump;
        _jumpSubscribed = false;

        // Si el hint de DASH ya esta pendiente (misma sesion de Level01), se omite
        // el de SALTO para no saturar al jugador: el de DASH ya lo cubre.
        if (!Seen(KEY_JUMP))
            ShowHint(KEY_JUMP, "W — SALTAR");
    }

    // ── Hint 3: ESCANEAR (proximidad a un Scannable) ──────────────────────
    private IEnumerator ScanProximityLoop()
    {
        var wait = new WaitForSeconds(ProximityCheckInterval);

        while (true)
        {
            yield return wait;

            if (_player == null) yield break;
            if (Seen(KEY_SCAN)) yield break;

            var scannables = FindObjectsByType<Scannable>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (var s in scannables)
            {
                if (s.YaEscaneado) continue;
                float dist = Vector2.Distance(
                    _player.transform.position, s.transform.position);

                if (dist <= ScanProximityRange)
                {
                    ShowHint(KEY_SCAN, "E — ESCANEAR OBJETO");
                    yield break;
                }
            }
        }
    }

    // ── API publica: Hint 4 (CELDA) lo llama EnergyCellPickup ─────────────
    /// <summary>
    /// Muestra el hint de "Q — usar celda" la primera vez que se recoge una celda.
    /// Llamado externamente por EnergyCellPickup en lugar de su ShowAlert directo.
    /// </summary>
    public static void NotifyCellPickedUp()
    {
        var sys = FindFirstObjectByType<TutorialHintSystem>();
        if (sys != null) sys.ShowHint(KEY_CELL_USE, "Q — USAR CELDA DE ENERGIA");
    }

    // ── Nucleo ────────────────────────────────────────────────────────────
    /// <summary>Muestra el hint si no se ha visto antes y marca como visto.</summary>
    private void ShowHint(string key, string message)
    {
        if (Seen(key)) return;
        MarkSeen(key);

        if (_hud != null)
            _hud.ShowAlert(message, HintDuration);
        else
            Debug.Log($"[TutorialHint] {message}");
    }

    // ── PlayerPrefs helpers ───────────────────────────────────────────────
    private static bool Seen(string key)     => PlayerPrefs.GetInt(key, 0) == 1;
    private static void MarkSeen(string key) { PlayerPrefs.SetInt(key, 1); PlayerPrefs.Save(); }

    // ── Limpieza de suscripciones ─────────────────────────────────────────
    private void UnsubscribePlayer()
    {
        if (_jumpSubscribed && _player != null)
            _player.OnJumped -= OnFirstJump;
        _jumpSubscribed = false;
        _player = null;

        if (_proximityRoutine != null)
        {
            StopCoroutine(_proximityRoutine);
            _proximityRoutine = null;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Resetea todos los hints desde el menu de Unity.
    /// Edit → Tutorial Hints → Reset All Hints
    /// </summary>
    [UnityEditor.MenuItem("Edit/Tutorial Hints/Reset All Hints")]
    private static void ResetAllHints()
    {
        PlayerPrefs.DeleteKey(KEY_DASH);
        PlayerPrefs.DeleteKey(KEY_JUMP);
        PlayerPrefs.DeleteKey(KEY_SCAN);
        PlayerPrefs.DeleteKey(KEY_CELL_USE);
        PlayerPrefs.Save();
        Debug.Log("[TutorialHintSystem] Todos los hints reseteados.");
    }
#endif
}
