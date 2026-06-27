using UnityEngine;

[CreateAssetMenu(fileName = "DronePatrollerStats",
menuName = "Enemies/Drone Patroller Stats")]
public class DronePatrollerStatsSO : ScriptableObject
{
    [Header("Health")]
    public int maxHp = 100;
    [Tooltip("Daño que recibe el dron cuando el rover lo embiste con dash (dash ofensivo).")]
    public int dashDamage = 40;

    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;

    [Header("Terrain Sensing")]
    [Tooltip("Distancia extra (más allá del borde del sprite) para detectar una pared al frente y girar.")]
    public float wallCheckDistance = 0.2f;
    [Tooltip("Margen frente a los pies para detectar el borde de una plataforma (evita caer al vacío).")]
    public float ledgeCheckDistance = 0.2f;
    [Tooltip("Radio máximo de patrulla desde el punto de aparición (0 = sin límite, solo paredes/bordes).")]
    public float patrolRange = 5f;

    [Header("Detection")]
    public float detectionRange = 7f;
    public float loseTime = 5f;

    [Header("Attack")]
    public float attackRange = 3.5f;
    public float attackCooldown = 1.5f;

    [Header("Projectile")]
    public float projectileSpeed = 8f;
    public float projectileLifetime = 2f;
}