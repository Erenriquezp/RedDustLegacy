using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStats",
    menuName = "Scriptable Objects/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    public float moveSpeed = 3.75f;
    public float alertRange = 5f;
    public float loseRange = 8f;
    public float loseTime = 3f;
    public float alertTime = 0.5f;
}