using UnityEngine;

[CreateAssetMenu(
    fileName = "CentinelaPrincipalStats",
    menuName = "AI/Centinela Principal Stats")]
public class CentinelaPrincipalStatsSO : ScriptableObject
{
    [Header("Vida")]
    public int maxHp = 400;             // GDD §8.7

    [Tooltip("Daño que recibe por cada embestida de dash del rover (400 HP → 8 dashes).")]
    public int dashDamage = 50;

    [Header("Movimiento")]
    public float moveSpeed = 1.5f;      // GDD §8.7: 1,5 u/s — no persigue activamente

    public float maxDistanceFromCenter = 3f; // GDD §8.7: anclado al centro

    [Header("Detección")]
    public float detectionRange = 10f;

    [Header("Ataque Fase 1")]
    public float attackRange = 5f;

    public float attackCooldown = 2.5f; // GDD §8.7 F1: 2,5 s

    public int projectileDamage = 12;   // GDD §8.7 F1: 12 SI

    public float projectileSpeed = 8f;  // GDD §8.7 F1: 8 u/s

    public int fanProjectiles = 3;      // GDD §8.7 F1: abanico ×3

    public float fanAngle = 30f;

    [Header("Ataque Fase 2")]
    public float phase2AttackCooldown = 2f;   // GDD §8.7 F2: 2,0 s

    public int phase2ProjectileDamage = 18;   // GDD §8.7 F2: 18 SI

    public int phase2Projectiles = 5;         // GDD §8.7 F2: abanico ×5

    public float phase2FanAngle = 45f;

    [Tooltip("Giro total máximo del proyectil rastreador de F2 (GDD §8.7: 45°).")]
    public float homingAngle = 45f;

    public float trackingProjectileSpeed = 6f; // GDD §8.7 F2: rastreo a 6 u/s

    [Header("Invocación")]
    public GameObject centinelaSecundarioPrefab;

    public float summonCooldown = 10f;

    [Header("Prefabs")]
    public GameObject projectilePrefab;
}
