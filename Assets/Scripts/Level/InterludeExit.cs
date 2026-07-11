using UnityEngine;

/// <summary>
/// Salida del interludio isométrico — S06 T1. Trigger 3D en la entrada del
/// Relicto: al cruzarlo el rover isométrico carga Level02 (con su pantalla de
/// carga). La herencia de SI/celdas viene pendiente desde el LevelExit de
/// Level01 y sobrevive el interludio (GameManager solo la consume en escenas
/// con DegradationSystem), así que aquí no hay nada que guardar.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InterludeExit : MonoBehaviour
{
    [Tooltip("Espera antes de cargar (para que el beat visual/sonoro respire).")]
    [SerializeField] private float loadDelay = 0.75f;

    private bool _triggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;

        // El Player isométrico va sin tag: se reconoce por su BasicCharacter
        // (se acepta también el tag Player por si se etiqueta más adelante).
        if (other.GetComponentInParent<Common.Scripts.BasicCharacter>() == null &&
            !other.CompareTag("Player"))
            return;

        _triggered = true;
        Invoke(nameof(LoadNext), Mathf.Max(0f, loadDelay));
    }

    private void LoadNext()
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadLevel02();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.8f);
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
