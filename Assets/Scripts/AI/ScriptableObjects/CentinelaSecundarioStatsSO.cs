using UnityEngine;

[CreateAssetMenu(fileName = "CentinelaSecundarioStats", menuName = "Enemy Stats/Centinela Secundario")]
public class CentinelaSecundarioStatsSO : ScriptableObject
{
    [Header("Vida")]
    public int maxHp = 100;

    [Tooltip("Daño que recibe por cada embestida de dash del rover (patrón S03).")]
    public int dashDamage = 50;

    [Header("Movimiento")]
    public float patrolSpeed = 2f;      // GDD §8.6: 2,0 u/s

    public float chaseSpeed = 3.5f;     // GDD §8.6: 3,5 u/s en Chase

    public float patrolDistance = 8f;

    [Tooltip("GDD §8.6: no se aleja más de 8 u de su punto de origen.")]
    public float leashRange = 8f;

    [Header("Detección")]
    public float detectionRange = 6f;   // GDD §8.6: omnidireccional, sin cono

    [Tooltip("Ya en persecución, no suelta al rover hasta que se aleje más de esto (> detectionRange).")]
    public float loseRange = 9f;

    [Header("Ataque")]
    public float attackRange = 5f;      // GDD §8.6: attackRange 5,0 u

    public float attackCooldown = 1.8f; // GDD §8.6: 1,8 s

    public int projectileDamage = 10;   // GDD §8.6: 10 SI

    public float projectileSpeed = 7.5f; // GDD §8.6: 7,5 u/s

    [Tooltip("Cada N.º disparo sale con rastreo parcial (GDD §8.6: cada 3er ataque).")]
    public int trackingShotEvery = 3;

    public float trackingAngle = 30f;   // GDD §8.6: gira hasta 30° hacia el rover

    [Header("Proyectil")]
    public GameObject projectilePrefab;

    public float projectileLifetime = 3f; // GDD §8.6: 3,0 s
}
