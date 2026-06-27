using UnityEngine;

public class CaidaPorCercania : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Configuración de la Trampa")]
    [Tooltip("Distancia en bloques hacia abajo para detectar al jugador")]
    public float distanciaDeteccion = 6f;

    [Header("Daño al impactar")]
    [Tooltip("SI que resta al golpear al Player tras caer.")]
    public float danioImpacto = 20f;

    private bool yaCayo = false;
    private bool yaDano = false;

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

    // Daña al Player una sola vez, tras empezar a caer.
    private void OnCollisionEnter2D(Collision2D collision) => TryDamage(collision.collider);
    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

    private void TryDamage(Collider2D other)
    {
        if (!yaCayo || yaDano || other == null || !other.CompareTag("Player")) return;

        var si = other.GetComponentInParent<DegradationSystem>();
        if (si != null)
        {
            si.TakeDamage(danioImpacto, transform.position);
            yaDano = true;
        }
    }

    // Esto dibuja una línea roja en la ventana 'Scene' para que veas el rango del láser
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distanciaDeteccion);
    }
}