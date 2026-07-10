using UnityEngine;

/// <summary>
/// Golpe de tentáculo del Leviatán (S04 T3, GDD §8.3): 15 SI con i-frames de
/// 0,8 s por fuente (el global del rover es 0,6 s). Los tentáculos NO reciben
/// daño — solo el núcleo (LeviatanCore). Usa Stay además de Enter para que el
/// DegradationSystem espacie el daño con sus i-frames si el rover se queda encima.
/// </summary>
public class LeviatanTentacle : MonoBehaviour
{
    [Header("Boss")]
    public LeviatanAI boss;

    private void OnTriggerEnter2D(Collider2D other) => DamageRover(other);
    private void OnTriggerStay2D(Collider2D other)  => DamageRover(other);

    private void DamageRover(Collider2D other)
    {
        if (boss == null || boss.IsDead) return;
        if (!other.CompareTag("Player")) return;

        DegradationSystem degradation = other.GetComponentInParent<DegradationSystem>();
        if (degradation == null) return;

        degradation.TakeDamage(
            boss.stats.tentacleDamage,
            transform.position,
            boss.stats.tentacleIFrames
        );
    }
}
