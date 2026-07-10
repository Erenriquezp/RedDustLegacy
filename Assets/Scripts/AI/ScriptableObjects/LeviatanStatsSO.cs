using UnityEngine;

[CreateAssetMenu(fileName = "LeviatanStats", menuName = "Enemies/Leviatan Stats")]
public class LeviatanStatsSO : ScriptableObject
{
    [Header("General")]
    public int maxHp = 200;
    public float moveSpeed = 0.8f;

    [Header("Intro (lockdown, GDD §8.3)")]
    [Tooltip("Segundos que dura la emersión antes de empezar a atacar.")]
    public float introDuration = 2f;

    [Header("Tentacles")]
    public int tentacleDamage = 15;
    public float attackCooldown = 2f;
    [Tooltip("Duración del ciclo de ataque si la animación no llama a FinishAttack().")]
    public float attackDuration = 1.2f;
    [Tooltip("i-frames del rover al recibir un tentáculo (GDD §8.3: 0,8 s; el global es 0,6 s).")]
    public float tentacleIFrames = 0.8f;

    [Header("Core (punto débil — dash ofensivo)")]
    [Tooltip("Daño base del dash del rover al núcleo. Con damageMultiplier ×2 ⇒ 50 ⇒ ~4 ventanas para 200 HP.")]
    public int dashDamage = 25;
    public float vulnerableTime = 2f;
    public float damageMultiplier = 2f;

    [Header("Enrage (<50% HP)")]
    public float enrageThreshold = 0.5f;
    public float enrageSpeedMultiplier = 1.5f;
}
