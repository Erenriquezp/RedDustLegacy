using UnityEngine;

[CreateAssetMenu(fileName = "LeviatanStats", menuName = "Enemies/Leviatan Stats")]
public class LeviatanStatsSO : ScriptableObject
{
    [Header("General")]
    public int maxHp = 200;
    public float moveSpeed = 0.8f;

    [Header("Tentacles")]
    public int tentacleDamage = 15;
    public float attackCooldown = 2f;

    [Header("Core")]
    public float vulnerableTime = 2f;
    public float damageMultiplier = 2f;

    [Header("Enrage")]
    public float enrageThreshold = 0.5f;
    public float enrageSpeedMultiplier = 1.5f;
}