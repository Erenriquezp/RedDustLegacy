using UnityEngine;

/// <summary>
/// Barra de carga segmentada de las pantallas de carga (S04 T1, HUD §10.2).
/// <see cref="SceneLoader"/> escribe <see cref="progresoCarga"/> cada frame;
/// este componente activa las barritas hijas, desliza la silueta del rover
/// (bot_id) a lo largo de la barra y reproduce ui_loading_tick por cada
/// segmento nuevo (GDD §12.2).
/// </summary>
public class ControladorBarraSegmentada : MonoBehaviour
{
    // Cambiamos el nombre a público directo para que Unity lo fuerce en el Inspector
    [Range(0f, 1f)]
    public float progresoCarga = 0f;

    [Header("Silueta del rover")]
    [Tooltip("Si se deja vacío, se busca un hermano llamado 'bot_id'.")]
    [SerializeField] private RectTransform silueta;

    private UIAudioController _uiAudio;
    private int _barritasActivas = -1;              // -1 = primer refresco sin tick
    private readonly Vector3[] _esquinas = new Vector3[4];

    private void Awake()
    {
        _uiAudio = FindFirstObjectByType<UIAudioController>(FindObjectsInactive.Include);
        if (silueta == null && transform.parent != null)
            silueta = transform.parent.Find("bot_id") as RectTransform;
    }

    void Update()
    {
        ActualizarBarritas();
        ActualizarSilueta();
    }

    private void ActualizarBarritas()
    {
        int totalBarritas = transform.childCount;
        int barritasAActivar = Mathf.RoundToInt(progresoCarga * totalBarritas);
        if (barritasAActivar == _barritasActivas) return;

        for (int i = 0; i < totalBarritas; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i < barritasAActivar);
        }

        // Tick por segmento nuevo (uno por frame aunque avancen varios de golpe).
        if (_barritasActivas >= 0 && barritasAActivar > _barritasActivas && _uiAudio != null)
            _uiAudio.PlayLoadingTickSound();

        _barritasActivas = barritasAActivar;
    }

    /// <summary>Desliza la silueta del borde izquierdo al derecho de la barra.</summary>
    private void ActualizarSilueta()
    {
        if (silueta == null || silueta.parent == null) return;

        // Bordes de la barra en el espacio local del padre de la silueta,
        // para no depender de anclas ni de la jerarquía concreta de la escena.
        ((RectTransform)transform).GetWorldCorners(_esquinas); // 0 = inf-izq, 3 = inf-der
        float izquierda = silueta.parent.InverseTransformPoint(_esquinas[0]).x;
        float derecha   = silueta.parent.InverseTransformPoint(_esquinas[3]).x;

        Vector3 pos = silueta.localPosition;
        pos.x = Mathf.Lerp(izquierda, derecha, progresoCarga);
        silueta.localPosition = pos;
    }
}
