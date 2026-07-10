using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("DDA")]
    [SerializeField] private bool ddaEnabled = false;

    [Header("Comunicación de drones (GDD §8.1)")]
    [Tooltip("Radio (u) alrededor del Detector dentro del cual los Patrulleros reciben la posición del rover.")]
    [SerializeField] private float shareRadius = 12f;

    private Vector2 lastKnownPlayerPosition;
    private bool playerDetected;


    [Header("DDA Settings")]
    [SerializeField] private float difficultyMultiplier = 1f;

    [SerializeField] private float minDifficulty = 0.75f;

    [SerializeField] private float maxDifficulty = 1.25f;

    private int deaths;

    private float survivalTimer;

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
        ReportPlayerDetected(playerPosition, playerPosition);
    }

    /// <summary>
    /// Un Detector confirma al rover: guarda la última posición conocida y la reparte
    /// a los Drones Patrulleros a menos de shareRadius del informante (GDD §8.1).
    /// </summary>
    public void ReportPlayerDetected(Vector2 playerPosition, Vector2 reporterPosition)
    {
        playerDetected = true;
        lastKnownPlayerPosition = playerPosition;

        Debug.Log($"AIManager: Rover detectado en {playerPosition}");

        SharePositionWithPatrollers(playerPosition, reporterPosition);
    }

    private void SharePositionWithPatrollers(Vector2 playerPosition, Vector2 reporterPosition)
    {
        var patrollers = FindObjectsByType<DronePatrollerAI>(FindObjectsSortMode.None);
        foreach (var patroller in patrollers)
        {
            if (Vector2.Distance(patroller.transform.position, reporterPosition) <= shareRadius)
                patroller.OnPlayerReported(playerPosition);
        }
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

    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector2 position)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("AIManager: Enemy Prefab es NULL.");
            return null;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            position,
            Quaternion.identity);

        return enemy;
    }

    private void Update()
    {
        if (!ddaEnabled)
            return;

        survivalTimer += Time.deltaTime;

        if (survivalTimer >= 480f) // 8 minutos
        {
            IncreaseDifficulty();

            survivalTimer = 0f;
        }
    }

    public void RegisterPlayerDeath()
    {
        if (!ddaEnabled)
            return;

        deaths++;

        survivalTimer = 0f;

        if (deaths >= 2)
        {
            DecreaseDifficulty();

            deaths = 0;
        }
    }

    private void IncreaseDifficulty()
{
    difficultyMultiplier += 0.1f;

    difficultyMultiplier =
        Mathf.Clamp(
            difficultyMultiplier,
            minDifficulty,
            maxDifficulty);

    Debug.Log(
        $"Dificultad aumentada: x{difficultyMultiplier:F2}");
}

private void DecreaseDifficulty()
{
    difficultyMultiplier -= 0.1f;

    difficultyMultiplier =
        Mathf.Clamp(
            difficultyMultiplier,
            minDifficulty,
            maxDifficulty);

    Debug.Log(
        $"Dificultad reducida: x{difficultyMultiplier:F2}");
}

public float GetDifficultyMultiplier()
{
    return difficultyMultiplier;
}


}