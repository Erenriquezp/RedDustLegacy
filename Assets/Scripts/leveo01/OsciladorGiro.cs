using UnityEngine;

public class OsciladorGiro : MonoBehaviour
{
    [Header("Configuración del Giro")]
    [Tooltip("El ángulo total que cubrirá el giro (ej. 180 grados)")]
    public float anguloDeGiro = 180f;

    [Tooltip("La velocidad a la que oscilará el objeto")]
    public float velocidadGiro = 1f;

    private Quaternion rotacionInicial;

    void Start()
    {
        // Guardamos la rotación inicial del pivote
        rotacionInicial = transform.rotation;
    }

    void Update()
    {
        // Usamos una función seno para obtener un valor suave entre -1 y 1
        float oscilacionVal = Mathf.Sin(Time.time * velocidadGiro);

        // Escalamos ese valor para que el giro vaya de -(angulo/2) a +(angulo/2)
        // Por ejemplo, si el ángulo es 180, oscilará entre -90 y 90 grados
        float anguloZ = oscilacionVal * (anguloDeGiro / 2f);

        // Aplicamos la nueva rotación local en el eje Z
        transform.rotation = rotacionInicial * Quaternion.Euler(0f, 0f, anguloZ);
    }
}
