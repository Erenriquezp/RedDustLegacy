using UnityEngine;

public class CaidaCristal : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Configuración de la Trampa")]
    public float distanciaDeteccion = 12f;

    // Nueva variable para decirle al rayo a quién buscar
    [Tooltip("Selecciona la capa 'Player' aquí en el Inspector")]
    public LayerMask capaObjetivo;

    [Header("Daño al impactar")]
    [Tooltip("SI que resta al golpear al Player tras caer.")]
    public float danioImpacto = 20f;

    private bool yaCayo = false;
    private bool yaDano = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (yaCayo) return;

        // El rayo ahora filtrará y SOLO se detendrá si choca contra la capa que le asignemos
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distanciaDeteccion, capaObjetivo);

        if (hit.collider != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            yaCayo = true;
        }
    }

    // El cristal solo daña una vez, y solo tras haber empezado a caer.
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * distanciaDeteccion);
    }
}