using UnityEngine;

[CreateAssetMenu(
    fileName = "CentinelaPrincipalStats",
    menuName = "AI/Centinela Principal Stats")]
public class CentinelaPrincipalStatsSO : ScriptableObject
{
    [Header("Vida")]
    public int maxHp = 400;

    [Header("Movimiento")]
    public float moveSpeed = 2f;

    public float maxDistanceFromCenter = 3f;

    [Header("Detección")]
    public float detectionRange = 10f;

    [Header("Ataque Fase 1")]
    public float attackCooldown = 2.5f;

    public int projectileDamage = 12;

    public float projectileSpeed = 8f;

    public int fanProjectiles = 3;

    public float fanAngle = 30f;

    [Header("Ataque Fase 2")]

    public int phase2ProjectileDamage = 18;

    public int phase2Projectiles = 5;

    public float homingAngle = 45f;

    [Header("Invocación")]

    public GameObject centinelaSecundarioPrefab;

    public float summonCooldown = 10f;

    [Header("Prefabs")]

    public GameObject projectilePrefab;
}