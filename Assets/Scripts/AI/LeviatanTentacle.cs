using UnityEngine;

public class LeviatanTentacle : MonoBehaviour
{
    [Header("Boss")]
    public LeviatanAI boss;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        DegradationSystem degradation =
            other.GetComponentInParent<DegradationSystem>();

        if (degradation == null)
            return;

        degradation.TakeDamage(
            boss.stats.tentacleDamage,
            transform.position
        );
    }
}