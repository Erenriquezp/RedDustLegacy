using UnityEngine;

/// <summary>
/// Singleton de escena que recuerda el último checkpoint (posición + SI) y
/// respawnea al jugador SIN recargar la escena — Sprint 03 T4 (GDD §7).
/// Enemigos derrotados y celdas recogidas NO reaparecen (no se resetea el nivel).
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public bool HasCheckpoint { get; private set; }
    public Vector3 RespawnPosition => _respawnPosition;
    public float SavedSI => _savedSI;

    private Vector3 _respawnPosition;
    private float _savedSI;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Lo llama un Checkpoint al activarse.</summary>
    public void Register(Vector3 position, float si)
    {
        _respawnPosition = position;
        _savedSI = si;
        HasCheckpoint = true;
    }

    /// <summary>Mueve al Player al último checkpoint y restaura la SI guardada (no a 100).</summary>
    public void RespawnPlayer(PlayerController player, DegradationSystem deg)
    {
        if (!HasCheckpoint || player == null || deg == null) return;

        player.transform.position = _respawnPosition;
        player.ResetMotion();
        deg.SetSI(_savedSI);
    }
}
