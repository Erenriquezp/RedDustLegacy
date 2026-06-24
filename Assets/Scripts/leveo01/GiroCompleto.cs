using UnityEngine;

public class GiroCompleto : MonoBehaviour
{
    [Header("Configuración del Giro Infinito")]
    [Tooltip("Velocidad de rotación continua. Positivo gira a la izquierda, negativo a la derecha.")]
    public float velocidadRotacion = 100f;

    void Update()
    {
        // transform.Rotate multiplica los grados por el tiempo para que el giro sea suave e infinito
        transform.Rotate(0f, 0f, velocidadRotacion * Time.deltaTime);
    }
}
