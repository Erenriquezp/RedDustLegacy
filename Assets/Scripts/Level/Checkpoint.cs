using System.Collections;
using UnityEngine;

/// <summary>
/// Trigger de checkpoint — Sprint 03 T4 (GDD §7). La primera vez que el Player lo
/// cruza, registra posición + SI en <see cref="CheckpointManager"/> y avisa por el HUD.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointID;
    [Tooltip("Desplazamiento del punto de respawn respecto al checkpoint.")]
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.5f);
    [Header("Reparación")]
    [Tooltip("Al cruzarlo, repara la SI hasta este valor (no la baja si ya está más alta). 0 = no reparar.")]
    [SerializeField] private float repairToSI = 74f;   // Fase 1 NOMINAL (GDD §4.1)
    [Tooltip("Opcional: se activa al registrar (luz/indicador encendido).")]
    [SerializeField] private GameObject activeIndicator;
    [Tooltip("Opcional: recibe un pulso de escala de 1.2 s al registrar.")]
    [SerializeField] private Transform pulseTarget;

    private bool _activated;

    private void Reset()
    {
        var box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(3f, 4f);   // GDD §7: ancho ≥3 u, alto 4 u
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated || !other.CompareTag("Player")) return;

        var deg = other.GetComponentInParent<DegradationSystem>();
        float si = deg != null ? deg.CurrentSI : 100f;

        // El checkpoint repara el rover hasta repairToSI (sin bajarlo si ya está más alto)
        // para que se pueda progresar. Se guarda la SI reparada, no la drenada.
        if (deg != null && repairToSI > 0f && si < repairToSI)
        {
            si = repairToSI;
            deg.SetSI(si);
        }

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.Register((Vector2)transform.position + respawnOffset, si);
        else
            Debug.LogWarning("[Checkpoint] No hay CheckpointManager en la escena; el registro se pierde.");

        _activated = true;

        if (activeIndicator != null) activeIndicator.SetActive(true);
        if (pulseTarget != null) StartCoroutine(PulseRoutine());

        var hud = FindFirstObjectByType<HUDManager>();
        if (hud != null) hud.ShowAlert("CHECKPOINT REGISTRADO", 2f);
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 baseScale = pulseTarget.localScale;
        Vector3 peak = baseScale * 1.4f;
        const float dur = 1.2f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float k = Mathf.Sin((t / dur) * Mathf.PI);   // sube y vuelve
            pulseTarget.localScale = Vector3.Lerp(baseScale, peak, k);
            yield return null;
        }
        pulseTarget.localScale = baseScale;
    }

    private void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider2D>();
        Gizmos.color = _activated ? Color.green : new Color(0.2f, 0.8f, 1f, 0.8f);
        if (box != null) Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + respawnOffset, 0.2f);
    }
}
