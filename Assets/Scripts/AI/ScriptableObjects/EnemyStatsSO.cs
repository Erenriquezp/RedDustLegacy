using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats",
                 menuName = "Scriptable Objects/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Health")]
    public int hp = 40;

    [Header("Damage")]
    public float contactDamage = 5f;

    [Header("Movement")]
    public float moveSpeedBase = 1.5f;
    public float moveSpeed = 3.75f;

    [Header("Detection")]
    public float alertRange = 5f;
    public float loseRange = 8f;
    public float loseTime = 3f;

    [Header("Alert")]
    public float alertTime = 0.5f;

    [Header("Vulnerability")]
    public float stunDuration = 2f;
}