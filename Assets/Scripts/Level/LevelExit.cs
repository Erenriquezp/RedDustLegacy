using UnityEngine;

/// <summary>
/// Salida de nivel — S05 (GDD §9.3, flujo N1→N2). Trigger al final del nivel:
/// al cruzarlo el Player guarda SI + celdas en <see cref="GameManager"/> (herencia
/// entre niveles) y carga el siguiente nivel vía <see cref="SceneLoader.LoadNextLevel"/>
/// (con su pantalla de carga). En Level01 debe empezar INACTIVO: lo activa
/// <see cref="BossArenaTrigger"/> al morir el Leviatán (exitToUnlock).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Tooltip("Aviso mostrado en el HUD al cruzar (vacío = sin aviso).")]
    [SerializeField] private string alertText = "SEÑAL DETECTADA — RUMBO AL RELICTO";
    [Tooltip("Espera antes de cargar, para que el aviso se lea.")]
    [SerializeField] private float loadDelay = 1.5f;

    private bool _triggered;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !other.CompareTag("Player")) return;
        _triggered = true;

        // Herencia de SI/celdas (GDD §9.3): el siguiente nivel arranca con lo que traes.
        var deg = other.GetComponentInParent<DegradationSystem>();
        if (deg != null && GameManager.Instance != null)
            GameManager.Instance.CarryOverToNextLevel(deg.CurrentSI, deg.CellsInReserve);

        if (!string.IsNullOrEmpty(alertText))
        {
            var hud = FindFirstObjectByType<HUDManager>();
            if (hud != null) hud.ShowAlert(alertText, loadDelay);
        }

        Invoke(nameof(LoadNext), Mathf.Max(0f, loadDelay));
    }

    private void LoadNext()
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadNextLevel();
    }

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider2D>();
        Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.8f);
        if (col is BoxCollider2D box)
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        else
            Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
