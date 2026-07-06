using UnityEngine;

[CreateAssetMenu(fileName = "DroneDetectorStats", menuName = "Enemy Stats/Drone Detector")]
public class DroneDetectorStatsSO : ScriptableObject
{
    [Header("Vida")]
    public int maxHp = 80;

    [Header("Movimiento")]
    public float moveSpeed = 3f;

    [Header("Visión")]
    public float visionDistance = 8f;

    [Range(0, 180)]
    public float visionAngle = 60f;

    public float alertTime = 1.5f;

    public float searchDuration = 6f;

    [Header("Ataque")]
    public float attackDistance = 1.5f;

    public float attackCooldown = 1f;
}