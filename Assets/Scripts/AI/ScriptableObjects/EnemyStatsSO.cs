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
    [Tooltip("Daño que recibe el enemigo cuando el rover lo embiste con dash (dash ofensivo).")]
    public int dashDamage = 20;

    [Header("Cadáver (S05 T1 — espécimen escaneable)")]
    [Tooltip("Segundos que el cadáver permanece en escena (escaneable) antes de desintegrarse.")]
    public float corpseDuration = 10f;

    [Tooltip("Ficha que muestra el escáner sobre el cadáver (p. ej. SC-B1). Si el objeto de la escena no trae un Scannable, se le añade uno al morir con esta ficha.")]
    public ScanDataSO fichaEscaneo;
}