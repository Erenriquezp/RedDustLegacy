using UnityEngine;

public class ControladorBarraSegmentada : MonoBehaviour
{
    // Cambiamos el nombre a público directo para que Unity lo fuerce en el Inspector
    [Range(0f, 1f)]
    public float progresoCarga = 0f;

    void Update()
    {
        int totalBarritas = transform.childCount;
        int barritasAActivar = Mathf.RoundToInt(progresoCarga * totalBarritas);

        for (int i = 0; i < totalBarritas; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i < barritasAActivar);
        }
    }
}