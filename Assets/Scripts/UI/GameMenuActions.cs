using UnityEngine;

/// <summary>
/// Relay de escena para los botones de Pausa / Game Over (Sprint 03 T5).
/// Los botones de UI no pueden referenciar directamente el singleton
/// DontDestroyOnLoad <see cref="GameManager"/> en el Inspector, así que enrutan
/// a través de este componente (que sí vive en la escena).
/// </summary>
public class GameMenuActions : MonoBehaviour
{
    public void Resume()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void Restart()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartFromCheckpoint();
    }

    public void ToMainMenu()
    {
        if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
    }
}
