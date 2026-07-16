using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton DontDestroyOnLoad que centraliza la carga de escenas.
/// S04 T1: los niveles se cargan a través de su pantalla de carga
/// (HUD §10.2, GDD §12.2) con AsyncOperation, barra de progreso real
/// y un tiempo mínimo en pantalla para que la telemetría se lea.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    // Nombres reales de las escenas en Build Settings.
    // Si se renombra una escena, actualizar aquí y en Build Settings.
    public const string MainMenuScene       = "MenuPrincipal";
    public const string Level01Scene        = "Level01";
    public const string Level02Scene        = "Level02";
    public const string IsometricScene      = "Isometric";
    public const string LoadingLevel01Scene = "PantallaCargaNivel1";
    public const string LoadingLevel02Scene = "PantallaCargaNivel2";

    [Header("Pantalla de carga (GDD §12.2)")]
    [Tooltip("Tiempo mínimo en la pantalla de carga aunque la carga sea instantánea.")]
    [SerializeField] private float minLoadingScreenTime = 2f;
    [Tooltip("Pausa con la barra llena antes de activar el nivel.")]
    [SerializeField] private float fullBarHold = 0.25f;

    private bool _isLoading;

    // ── Bootstrap automático (mismo patrón que GameManager) ───────────────
    // Garantiza que exista aunque la escena no tenga un objeto SceneLoader
    // (p. ej. al entrar a Play directamente en Level01/Level02).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("SceneLoader");
        go.AddComponent<SceneLoader>();
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMainMenu()
    {
        if (_isLoading) return;
        SceneManager.LoadScene(MainMenuScene);
    }

    public void LoadLevel01() => LoadLevelWithLoadingScreen(LoadingLevel01Scene, Level01Scene);
    public void LoadLevel02() => LoadLevelWithLoadingScreen(LoadingLevel02Scene, Level02Scene);

    /// <summary>
    /// Interludio isométrico N1→N2 (S06 T1). Comparte la pantalla de carga del
    /// Relicto (temáticamente "rumbo al Relicto"); si el Artist crea una pantalla
    /// propia del interludio, cambiar aquí la escena de carga.
    /// </summary>
    public void LoadIsometric() => LoadLevelWithLoadingScreen(LoadingLevel02Scene, IsometricScene);

    /// <summary>
    /// Avanza por nombre (Level01 → Level02 → menú). El interludio isométrico
    /// está desactivado del flujo automático; se puede cargar con LoadIsometric().
    /// </summary>
    public void LoadNextLevel()
    {
        string current = SceneManager.GetActiveScene().name;
        if (current == Level01Scene)        LoadLevel02();
        else if (current == IsometricScene) LoadLevel02();
        else if (current == Level02Scene)   LoadMainMenu();
        else                                LoadLevel01();
    }

    public void ReloadCurrentScene()
    {
        if (_isLoading) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadLevelWithLoadingScreen(string loadingScene, string targetScene)
    {
        if (_isLoading) return;
        StartCoroutine(LoadingRoutine(loadingScene, targetScene));
    }

    private IEnumerator LoadingRoutine(string loadingScene, string targetScene)
    {
        _isLoading = true;

        // 1. Mostrar la pantalla de carga (escena ligera, carga síncrona)
        //    y esperar un frame a que existan sus objetos.
        SceneManager.LoadScene(loadingScene);
        yield return null;

        var barra = FindFirstObjectByType<ControladorBarraSegmentada>(FindObjectsInactive.Include);

        // 2. Cargar el nivel sin activarlo. Con allowSceneActivation = false,
        //    op.progress se detiene en 0.9 → se normaliza a 0–1.
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        float elapsed = 0f;
        while (op.progress < 0.9f || elapsed < minLoadingScreenTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float realProgress  = Mathf.Clamp01(op.progress / 0.9f);
            float timedProgress = Mathf.Clamp01(elapsed / minLoadingScreenTime);
            if (barra != null) barra.progresoCarga = Mathf.Min(realProgress, timedProgress);
            yield return null;
        }

        if (barra != null) barra.progresoCarga = 1f;
        yield return new WaitForSecondsRealtime(fullBarHold);

        // 3. Activar el nivel.
        op.allowSceneActivation = true;
        _isLoading = false;
    }
}
