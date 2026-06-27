using System;
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

    private const string MAIN_MENU_SCENE = "MainMenu";

    private DegradationSystem _degradation;
    private HUDManager _hud;

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
    }

    // ── Carga de escena: re-localizar referencias y fijar estado ──────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        UnsubscribeDegradation();
        _degradation = FindFirstObjectByType<DegradationSystem>(FindObjectsInactive.Include);
        if (_degradation != null) _degradation.OnDeath += HandlePlayerDeath;

        _hud = FindFirstObjectByType<HUDManager>(FindObjectsInactive.Include);
        if (_hud != null) _hud.ShowPause(false);

        SetState(scene.name == MAIN_MENU_SCENE ? GameState.MainMenu : GameState.Playing);
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
        Time.timeScale = 0f;
        if (_hud != null) _hud.ShowGameOver();
        if (Core.AudioManager.Instance != null) Core.AudioManager.Instance.TriggerGameOverMusic();
        SetState(GameState.GameOver);
    }

    /// <summary>
    /// Reintentar. Si hay un checkpoint registrado (T4), respawnea SIN recargar
    /// (no resetea el nivel). Si no, recarga la escena (reinicio desde el principio).
    /// </summary>
    public void RestartFromCheckpoint()
    {
        Time.timeScale = 1f;

        var cm = CheckpointManager.Instance;
        if (cm != null && cm.HasCheckpoint)
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

    // ── Internos ──────────────────────────────────────────────────────────
    private void SetState(GameState state)
    {
        CurrentState = state;
        OnStateChanged?.Invoke(state);
    }

    private void UnsubscribeDegradation()
    {
        if (_degradation != null) _degradation.OnDeath -= HandlePlayerDeath;
        _degradation = null;
    }
}
