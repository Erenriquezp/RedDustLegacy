using UnityEngine;

[CreateAssetMenu(fileName = "LeviatanStats", menuName = "Enemies/Leviatan Stats")]
public class LeviatanStatsSO : ScriptableObject
{
    [Header("General")]
    public int maxHp = 200;
    [Tooltip("Velocidad de persecución. En enrage se multiplica por enrageSpeedMultiplier.")]
    public float moveSpeed = 2.2f;

    [Header("Intro (lockdown, GDD §8.3)")]
    [Tooltip("Segundos que dura la emersión antes de empezar a atacar.")]
    public float introDuration = 2f;

    [Header("Tentacles")]
    public int tentacleDamage = 15;
    public float attackCooldown = 1.2f;
    [Tooltip("Duración del ciclo de ataque si la animación no llama a FinishAttack().")]
    public float attackDuration = 1.2f;
    [Tooltip("i-frames del rover al recibir un tentáculo (GDD §8.3: 0,8 s; el global es 0,6 s).")]
    public float tentacleIFrames = 0.8f;

    [Header("Embestida (salto rápido que cae frente al rover — S05)")]
    [Tooltip("A menos de esta distancia no salta: usa el combo de tentáculos.")]
    public float lungeMinRange = 5f;
    [Tooltip("A más de esta distancia no salta: camina para acercarse.")]
    public float lungeMaxRange = 12f;
    [Tooltip("Segundos entre embestidas. En enrage se divide por enrageSpeedMultiplier.")]
    public float lungeCooldown = 5f;
    [Tooltip("Aviso (tinte + rugido) antes de saltar: la ventana del jugador para moverse.")]
    public float lungeTelegraphTime = 0.5f;
    [Tooltip("Segundos en el aire. En enrage se divide por enrageSpeedMultiplier.")]
    public float lungeDuration = 0.55f;
    public float lungeArcHeight = 3.5f;
    [Tooltip("Cae a esta distancia DELANTE del rover (del lado desde el que venía), nunca encima.")]
    public float lungeLandingOffset = 2.2f;
    [Tooltip("Daño de la onda de impacto al aterrizar (entre tentáculo 15 y castigo mayor).")]
    public int lungeDamage = 20;
    [Tooltip("Radio de la onda de impacto alrededor del punto de aterrizaje.")]
    public float lungeImpactRadius = 2.8f;

    [Header("Core (punto débil — dash ofensivo)")]
    [Tooltip("Daño base del dash del rover al núcleo. Con damageMultiplier ×2 ⇒ 50 ⇒ ~4 ventanas para 200 HP.")]
    public int dashDamage = 25;
    public float vulnerableTime = 2f;
    public float damageMultiplier = 2f;

    [Header("Enrage (<50% HP)")]
    public float enrageThreshold = 0.5f;
    public float enrageSpeedMultiplier = 1.5f;
}
