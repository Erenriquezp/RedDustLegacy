using UnityEngine;

public class CaidaPorCercania : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Configuración de la Trampa")]
    [Tooltip("Distancia en bloques hacia abajo para detectar al jugador")]
    public float distanciaDeteccion = 6f;

    private bool yaCayo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Al inicio congelamos el cristal para que no se caiga solo
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (yaCayo) return;

        // Lanzamos un rayo invisible desde el cristal hacia abajo en el eje Y
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distanciaDeteccion);

        // Si el rayo choca con algo y ese algo es el Player...
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // Cambiamos a Dynamic para que la gravedad real de Unity lo haga caer de golpe
            rb.bodyType = RigidbodyType2D.Dynamic;
            yaCayo = true;
        }
    }

    // Esto dibuja una línea roja en la ventana 'Scene' para que veas el rango del láser
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distanciaDeteccion);
    }
}