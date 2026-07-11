using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Objeto escaneable de escena (S05 T1). Colocar con un Collider2D trigger en la
/// capa Interactable y asignar su ScanDataSO. El gating (abrir puerta SC-03,
/// activar ascensor SC-05…) se cablea en onScanned desde el Inspector — sin
/// código nuevo por objeto. Guía: Docs/Architecture/ScanSystem.md.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Scannable : MonoBehaviour
{
    public ScanDataSO data;

    [Tooltip("Se dispara al COMPLETAR el escaneo (gating: puertas, ascensor, rutas). Con unaVez, solo la primera.")]
    public UnityEvent onScanned;

    public bool YaEscaneado { get; private set; }

    private void Reset()
    {
        // Autoconfiguración al añadir el componente en el editor.
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) gameObject.layer = layer;

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    /// <summary>
    /// Lo llama ScanSystem al completar la adquisición. Devuelve true si es la
    /// primera vez (para alertas/flashback/códex). Los repetibles (unaVez=false)
    /// re-disparan onScanned en cada escaneo.
    /// </summary>
    public bool Consume()
    {
        bool first = !YaEscaneado;
        YaEscaneado = true;

        if (first || (data != null && !data.unaVez))
            onScanned?.Invoke();

        return first;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.9f);
        var col = GetComponent<Collider2D>();
        if (col != null) Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
