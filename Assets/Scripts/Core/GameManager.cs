using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Máquina de estados global del juego — Sprint 03 T5 (GDD §15.1).
/// Singleton DontDestroyOnLoad. Coordina pausa, muerte→Game Over y respawn.
/// Punto de contacto entre DegradationSystem, HUDManager, SceneLoader y AudioManager.
/// Se autoarranca (no requiere colocarlo en escena) y re-localiza las referencias
/// por escena en cada carga.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver, Cinematic }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public event Action<GameState> OnStateChanged;

    private const string MAIN_MENU_SCENE = SceneLoader.MainMenuScene;

    /// <summary>Escenas de UI donde no hay gameplay (menú, pantallas de carga, pausa aislada).</summary>
    private static readonly string[] MENU_SCENES =
    {
        SceneLoader.MainMenuScene,
        SceneLoader.LoadingLevel01Scene,
        SceneLoader.LoadingLevel02Scene,
        "StopMenu",
    };

    private DegradationSystem _degradation;
    private HUDManager _hud;
    private ScanSystem _scan;

    // ── Lore escaneado (S06 T5): sobrevive entre escenas y se persiste en el
    // guardado; lo consultan Archivo de Misión (T3) y códex (T4).
    private readonly HashSet<string> _scannedIds = new HashSet<string>();

    // ── Herencia entre niveles (S05, GDD §9.3): SI/celdas con las que se entra
    // al siguiente nivel. Las guarda LevelExit; se consumen al cargar el nivel.
    private float _pendingSI = -1f;
    private int _pendingCells = -1;

    /// <summary>La llama <see cref="LevelExit"/> al cruzar la salida de un nivel.</summary>
    public void CarryOverToNextLevel(float si, int cells)
    {
        _pendingSI = si;
        _pendingCells = cells;
    }

    // ── Bootstrap automático ──────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeDegradation();
        UnsubscribeScan();
    }

    // ── Carga de escena: re-localizar referencias y fijar estado ──────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        UnsubscribeDegradation();
        _degradation = FindFirstObjectByType<DegradationSystem>(FindObjectsInactive.Include);
        if (_degradation != null)
        {
            _degradation.OnDeath += HandlePlayerDeath;
            // Autosave al recoger/usar celda (S06 T5): sin esto, una celda tomada
            // después del último checkpoint se perdería del guardado al salir.
            _degradation.OnCellsChanged += HandleCellsChanged;
        }

        UnsubscribeScan();
        _scan = FindFirstObjectByType<ScanSystem>(FindObjectsInactive.Include);
        if (_scan != null) _scan.OnScanCompleted += HandleScanCompleted;

        _hud = FindFirstObjectByType<HUDManager>(FindObjectsInactive.Include);
        if (_hud != null) _hud.ShowPause(false);

        bool isMenuScene = Array.IndexOf(MENU_SCENES, scene.name) >= 0;

        // Herencia de SI/celdas (GDD §9.3): se aplica una vez al entrar al nivel.
        // Volver al menú principal la descarta (partida nueva = valores por defecto);
        // las pantallas de carga intermedias NO la descartan.
        if (scene.name == MAIN_MENU_SCENE)
        {
            _pendingSI = -1f;
            _pendingCells = -1;
        }
        else if (!isMenuScene && _degradation != null)
        {
            if (_pendingSI >= 0f)  { _degradation.SetSI(_pendingSI); _pendingSI = -1f; }
            if (_pendingCells >= 0) { _degradation.SetCells(_pendingCells); _pendingCells = -1; }
        }

        // HUD y GameManager deben mirar a la MISMA instancia; re-sincroniza la barra
        // con el estado final (herencia ya aplicada).
        if (_hud != null && !isMenuScene) _hud.Bind(_degradation);

        // Autosave al entrar a un nivel (S06 T5): cubre "al completar nivel"
        // (se guarda al pisar el siguiente) y el arranque de NUEVA MISIÓN.
        AutoSave();

        SetState(isMenuScene ? GameState.MainMenu : GameState.Playing);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.Paused) return;

        bool pausePressed =
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

        if (pausePressed) TogglePause();
    }

    // ── API de estados ────────────────────────────────────────────────────
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing) PauseGame();
        else if (CurrentState == GameState.Paused) ResumeGame();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        Time.timeScale = 0f;
        if (_hud != null) _hud.ShowPause(true);
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        if (_hud != null) _hud.ShowPause(false);
        SetState(GameState.Playing);
    }

    private void HandlePlayerDeath()
    {
        if (CurrentState == GameState.GameOver) return;

        // Contadores de muerte: intentos del checkpoint (reinicio de nivel al
        // agotarse) y DDA del AIManager (S05 T3) si está en la escena.
        if (CheckpointManager.Instance != null) CheckpointManager.Instance.RegisterDeath();
        if (AIManager.Instance != null) AIManager.Instance.RegisterPlayerDeath();

        // El estado cambia YA (bloquea pausa y evita que el freeze del dash
        // "descongele" el juego), pero el freeze + pantalla esperan a que la
        // animación de muerte del rover se vea (antes se congelaba en el frame 0).
        SetState(GameState.GameOver);
        StartCoroutine(GameOverSequence());
    }

    /// <summary>Segundos de animación de muerte visibles antes de congelar y mostrar Game Over.</summary>
    private const float GameOverFreezeDelay = 1.5f;

    private IEnumerator GameOverSequence()
    {
        // Realtime: si la muerte ocurrió con el timeScale ya en 0 (freeze del
        // dash), un WaitForSeconds escalado no avanzaría y el Game Over jamás
        // aparecería.
        yield return new WaitForSecondsRealtime(GameOverFreezeDelay);

        // Si el estado ya no es Game Over (p. ej. se recargó la escena), abortar.
        if (CurrentState != GameState.GameOver) yield break;

        Time.timeScale = 0f;
        if (_hud != null) _hud.ShowGameOver();
        if (Core.AudioManager.Instance != null) Core.AudioManager.Instance.TriggerGameOverMusic();
    }

    /// <summary>
    /// Reintentar. Si hay un checkpoint registrado (T4), respawnea SIN recargar
    /// (no resetea el nivel). Sin checkpoint — o con los intentos del checkpoint
    /// agotados (3 muertes seguidas) — recarga la escena: nivel desde el
    /// principio con SI y estado de arranque.
    /// </summary>
    public void RestartFromCheckpoint()
    {
        Time.timeScale = 1f;

        var cm = CheckpointManager.Instance;
        if (cm != null && cm.HasCheckpoint && !cm.AttemptsExhausted)
        {
            var player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
            var deg = _degradation != null
                ? _degradation
                : FindFirstObjectByType<DegradationSystem>(FindObjectsInactive.Include);

            if (player != null && deg != null)
            {
                cm.RespawnPlayer(player, deg);
                if (_hud != null) { _hud.HideGameOver(); _hud.ShowPause(false); }
                SetState(GameState.Playing);
                return;
            }
        }

        // Sin checkpoint: recarga la escena actual.
        if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadCurrentScene();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadMainMenu();
        else SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    /// <summary>Congela/descongela el control del jugador para cinemáticas (GDD §15.1).</summary>
    public void SetCinematic(bool active)
    {
        Time.timeScale = 1f;
        SetState(active ? GameState.Cinematic : GameState.Playing);
    }

    // ── Guardado + CONTINUAR real (S06 T5) ────────────────────────────────

    /// <summary>Fichas SC-XX ya escaneadas en esta partida (para Archivo/códex, T3/T4).</summary>
    public bool IsScanned(string id) => _scannedIds.Contains(id);
    public IReadOnlyCollection<string> ScannedIds => _scannedIds;

    /// <summary>
    /// Autosave de slot único: al entrar a un nivel o al interludio (lo llama
    /// OnSceneLoaded), al registrar checkpoint (CheckpointManager) y en cada
    /// cambio de celdas. Las escenas sandbox (Dev/Enemy) no pisan el slot.
    /// </summary>
    public void AutoSave()
    {
        string scene = SceneManager.GetActiveScene().name;
        bool isLevel = scene == SceneLoader.Level01Scene || scene == SceneLoader.Level02Scene;
        // La escena isométrica ya no forma parte del flujo automático;
        // no se usa como punto de guardado.
        if (!isLevel) return;

        // Partir del slot existente para no pisar campos de otros sistemas
        // (opciones/dificultad T2, upgrades S05 T5, códex T4).
        var data = SaveSystem.Load() ?? new SaveData();
        data.sceneName = scene;
        if (_degradation != null)
        {
            data.si = _degradation.CurrentSI;
            data.cells = _degradation.CellsInReserve;
        }
        data.scannedIds = new List<string>(_scannedIds);
        SaveSystem.Save(data);
    }

    /// <summary>
    /// CONTINUAR del menú: restaura lore + SI/celdas (vía la herencia pendiente,
    /// que la pantalla de carga no descarta) y carga la escena guardada.
    /// </summary>
    public void ContinueFromSave()
    {
        var data = SaveSystem.Load();
        if (data == null) { StartNewGame(); return; }

        _scannedIds.Clear();
        if (data.scannedIds != null)
            foreach (var id in data.scannedIds) _scannedIds.Add(id);

        CarryOverToNextLevel(data.si, data.cells);

        if (SceneLoader.Instance == null) { SceneManager.LoadScene(data.sceneName); return; }
        if (data.sceneName == SceneLoader.Level02Scene) SceneLoader.Instance.LoadLevel02();
        // La escena isométrica ya no es un punto de guardado; si hay un save
        // antiguo que apuntaba a ella, redirigir a Level02.
        else if (data.sceneName == SceneLoader.IsometricScene) SceneLoader.Instance.LoadLevel02();
        else SceneLoader.Instance.LoadLevel01();
    }

    /// <summary>NUEVA MISIÓN: borra el slot (la UI ya confirmó) y arranca Level01 limpio.</summary>
    public void StartNewGame()
    {
        SaveSystem.Delete();
        _scannedIds.Clear();
        _pendingSI = -1f;
        _pendingCells = -1;

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadLevel01();
        else SceneManager.LoadScene(SceneLoader.Level01Scene);
    }

    private void HandleScanCompleted(ScanDataSO data)
    {
        if (data != null && !string.IsNullOrEmpty(data.id)) _scannedIds.Add(data.id);
    }

    /// <summary>
    /// Cada cambio de reserva persiste el estado actual (SI + celdas coherentes:
    /// usar una celda guarda la SI ya restaurada). AutoSave ignora escenas no-nivel.
    /// </summary>
    private void HandleCellsChanged(int _) => AutoSave();

    // ── Internos ──────────────────────────────────────────────────────────
    private void SetState(GameState state)
    {
        CurrentState = state;
        OnStateChanged?.Invoke(state);
    }

    private void UnsubscribeDegradation()
    {
        if (_degradation != null)
        {
            _degradation.OnDeath -= HandlePlayerDeath;
            _degradation.OnCellsChanged -= HandleCellsChanged;
        }
        _degradation = null;
    }

    private void UnsubscribeScan()
    {
        if (_scan != null) _scan.OnScanCompleted -= HandleScanCompleted;
        _scan = null;
    }
}
