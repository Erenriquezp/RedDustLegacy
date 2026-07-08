using UnityEngine;

[CreateAssetMenu(fileName = "DroneDetectorStats", menuName = "Enemy Stats/Drone Detector")]
public class DroneDetectorStatsSO : ScriptableObject
{
    [Header("Vida")]
    public int maxHp = 80;

    [Header("Movimiento (GDD §8.5)")]
    [Tooltip("Velocidad en Chase/Search.")]
    public float moveSpeed = 4.5f;
    [Tooltip("Velocidad de patrullaje.")]
    public float patrolSpeed = 2.5f;
    [Tooltip("Distancia máxima (u) a cada lado del origen al patrullar.")]
    public float patrolRange = 4f;

    [Header("Visión")]
    public float visionDistance = 8f;

    [Range(0, 180)]
    public float visionAngle = 60f;

    public float alertTime = 1.5f;

    public float searchDuration = 6f;

    [Header("Ataque (GDD §8.5: proyectil recto 8–12 SI)")]
    public float attackDistance = 4f;

    public float attackCooldown = 2f;

    public float projectileSpeed = 7f;

    public float projectileLifetime = 2.5f;

    [Header("Combate (patrón dash ofensivo S03)")]
    public int dashDamage = 40;
}