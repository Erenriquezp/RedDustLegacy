using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("DDA")]
    [SerializeField] private bool ddaEnabled = false;

    private Vector2 lastKnownPlayerPosition;
    private bool playerDetected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ReportPlayerDetected(Vector2 playerPosition)
    {
        playerDetected = true;
        lastKnownPlayerPosition = playerPosition;

        Debug.Log($"AIManager: Rover detectado en {playerPosition}");
    }

    public Vector2 GetLastKnownPlayerPosition()
    {
        return lastKnownPlayerPosition;
    }

    public bool HasPlayerBeenDetected()
    {
        return playerDetected;
    }

    public void ClearDetection()
    {
        playerDetected = false;
    }

    public bool IsDDAEnabled()
    {
        return ddaEnabled;
    }
}