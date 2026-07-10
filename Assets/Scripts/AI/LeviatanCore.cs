using UnityEngine;

/// <summary>
/// Punto débil del Leviatán (S04 T3, GDD §8.3). Hijo del boss con Collider2D
/// (trigger); LeviatanAI lo activa solo durante Vulnerable. Recibe el dash
/// ofensivo del rover (mismo patrón que Biol/Drone) y aplica el daño al boss
/// con su multiplicador ×2.
/// </summary>
public class LeviatanCore : MonoBehaviour
{
    public LeviatanAI boss;

    private float dashHitCooldown = 0.4f;   // un golpe por dash, no por frame de física
    private float dashHitTimer;

    private void OnTriggerStay2D(Collider2D other)  => DamageOnContact(other);
    private void OnCollisionStay2D(Collision2D c)   => DamageOnContact(c.collider);

    private void DamageOnContact(Collider2D other)
    {
        if (boss == null || !other.CompareTag("Player")) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null || !player.IsDashing) return;

        if (Time.time - dashHitTimer < dashHitCooldown) return;
        dashHitTimer = Time.time;

        boss.TakeCoreDamage(boss.stats.dashDamage);
    }

    /// <summary>Vía alternativa para otras fuentes de daño (p. ej. AnimationEvent o debug).</summary>
    public void TakeDamage(int damage)
    {
        if (boss != null) boss.TakeCoreDamage(damage);
    }
}
