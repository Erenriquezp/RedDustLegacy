using UnityEngine;

[CreateAssetMenu(fileName = "CentinelaSecundarioStats", menuName = "Enemy Stats/Centinela Secundario")]
public class CentinelaSecundarioStatsSO : ScriptableObject
{
    [Header("Vida")]
    public int maxHp = 100;

    [Header("Movimiento")]
    public float patrolSpeed = 2f;

    public float patrolDistance = 8f;

    [Header("Detección")]
    public float detectionRange = 6f;

    [Header("Ataque")]
    public float attackCooldown = 1.2f;

    public float projectileSpeed = 10f;

    public float trackingAngle = 30f;
    [Header("Proyectil")]
    public GameObject projectilePrefab;

    public float projectileLifetime = 5f;
}