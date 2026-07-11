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

    [Tooltip("Radio omnidireccional: a esta distancia detecta al rover aunque esté fuera del cono (las paredes siguen bloqueando).")]
    public float proximityRadius = 3.5f;

    public float alertTime = 1.5f;

    [Tooltip("Gracia (s) antes de abandonar Alert si pierde el contacto un instante (saltos del rover).")]
    public float alertLoseGrace = 1f;

    [Tooltip("Ya en Chase, persigue mientras el rover esté a menos de esta distancia, sin exigir visión continua.")]
    public float chaseRange = 11f;

    public float searchDuration = 6f;

    [Header("Ataque (GDD §8.5: proyectil recto 8–12 SI)")]
    public float attackDistance = 4f;

    public float attackCooldown = 2f;

    public float projectileSpeed = 7f;

    public float projectileLifetime = 2.5f;

    [Header("Combate (patrón dash ofensivo S03)")]
    public int dashDamage = 40;
}