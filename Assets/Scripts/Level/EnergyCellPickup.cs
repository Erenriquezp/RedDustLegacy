using UnityEngine;

/// <summary>
/// Pickup de celda de energía — Sprint 05 T5 (GDD §5). Al cruzarlo, el Player añade
/// una celda a la reserva del <see cref="DegradationSystem"/> (máx. 2, se usan con Q).
/// Si la reserva está llena NO se consume: queda en el mundo para volver a por ella.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class EnergyCellPickup : MonoBehaviour
{
    [Tooltip("Opcional: SFX global al recoger (vía AudioManager).")]
    [SerializeField] private AudioClip pickupSfx;

    private void Reset()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var deg = other.GetComponentInParent<DegradationSystem>();
        if (deg == null) return;

        // Reserva llena: avisa y deja el pickup en el mundo (GDD §5: máx. 2 en reserva).
        if (!deg.AddCell())
        {
            var hudFull = FindFirstObjectByType<HUDManager>();
            if (hudFull != null) hudFull.ShowAlert("RESERVA DE CELDAS LLENA", 1.5f);
            return;
        }

        if (pickupSfx != null && Core.AudioManager.Instance != null)
            Core.AudioManager.Instance.PlayGlobalSFX(pickupSfx);

        var hud = FindFirstObjectByType<HUDManager>();
        if (hud != null) hud.ShowAlert("CELDA DE ENERGIA RECOGIDA — Q PARA USAR", 2f);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
