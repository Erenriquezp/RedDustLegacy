using UnityEngine;

/// <summary>
/// Aplica daño de SI al Player al tocar un hazard (Danger_Tilemap, obstáculos
/// giratorios, pinchos…). Soporta collider sólido o trigger, y dos modos:
/// continuo (daño/seg mientras hay contacto) o por golpe (ráfaga con cooldown).
/// Colócalo en el GameObject que tenga el Collider2D del hazard.
/// </summary>
public class HazardDamage : MonoBehaviour
{
    [Header("Modo")]
    [Tooltip("Continuo = daño por segundo mientras toca; desmarcado = un golpe con cooldown.")]
    [SerializeField] private bool continuo = true;

    [Header("Valores")]
    [Tooltip("Daño por segundo en modo continuo (p. ej. Danger_Tilemap).")]
    [SerializeField] private float danioPorSegundo = 10f;
    [Tooltip("Daño por golpe en modo no-continuo (p. ej. obstáculo giratorio).")]
    [SerializeField] private float danioPorGolpe = 15f;
    [Tooltip("Segundos entre golpes en modo no-continuo (i-frames del hazard).")]
    [SerializeField] private float cooldownGolpe = 1f;

    private float _ultimoGolpe = -999f;

    // ── Continuo (mientras toca) ──────────────────────────────────────────
    private void OnTriggerStay2D(Collider2D other)   => Continuo(other);
    private void OnCollisionStay2D(Collision2D c)    => Continuo(c.collider);

    // ── Golpe (al entrar en contacto) ─────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)  => Golpe(other);
    private void OnCollisionEnter2D(Collision2D c)   => Golpe(c.collider);

    private void Continuo(Collider2D other)
    {
        if (!continuo) return;
        var si = Resolve(other);
        // Golpe plano: los i-frames del rover lo espacian (ya no daño/frame en cascada).
        if (si != null) si.TakeDamage(danioPorSegundo, transform.position);
    }

    private void Golpe(Collider2D other)
    {
        if (continuo) return;
        if (Time.time - _ultimoGolpe < cooldownGolpe) return;
        var si = Resolve(other);
        if (si != null)
        {
            si.TakeDamage(danioPorGolpe, transform.position);
            _ultimoGolpe = Time.time;
        }
    }

    private static DegradationSystem Resolve(Collider2D other)
    {
        if (other == null || !other.CompareTag("Player")) return null;
        return other.GetComponentInParent<DegradationSystem>();
    }
}
