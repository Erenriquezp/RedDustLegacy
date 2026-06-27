using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    public enum TipoMovimiento { Horizontal, Vertical }

    [Header("Configuración de Tipo de Movimiento")]
    [Tooltip("Elige si se mueve de lado a lado (Horizontal) o de arriba a abajo (Vertical)")]
    public TipoMovimiento movimiento = TipoMovimiento.Horizontal;

    [Header("Configuración de Distancia y Velocidad")]
    [Tooltip("La distancia máxima que se moverá la plataforma desde su origen")]
    public float distanciaMovimiento = 5f;
    [Tooltip("Velocidad a la que se desplazará")]
    public float velocidad = 2f;

    [Header("Dirección Inicial")]
    [Tooltip("Si es Horizontal: Marcado = Izquierda, Desmarcado = Derecha.\nSi es Vertical: Marcado = Abajo, Desmarcado = Arriba.")]
    public bool iniciarNegativo = true;

    private Vector3 posicionInicial;
    private int direccion = -1;
    private float limiteMin;
    private float limiteMax;

    // Player que va montado encima (para arrastrarlo con la plataforma).
    private Transform pasajero;

    void Start()
    {
        posicionInicial = transform.position;

        if (movimiento == TipoMovimiento.Horizontal)
        {
            direccion = iniciarNegativo ? -1 : 1;
            limiteMin = iniciarNegativo ? posicionInicial.x - distanciaMovimiento : posicionInicial.x;
            limiteMax = iniciarNegativo ? posicionInicial.x : posicionInicial.x + distanciaMovimiento;
        }
        else // Vertical
        {
            direccion = iniciarNegativo ? -1 : 1;
            limiteMin = iniciarNegativo ? posicionInicial.y - distanciaMovimiento : posicionInicial.y;
            limiteMax = iniciarNegativo ? posicionInicial.y : posicionInicial.y + distanciaMovimiento;
        }
    }

    void Update()
    {
        Vector3 antes = transform.position;

        if (movimiento == TipoMovimiento.Horizontal)
        {
            transform.Translate(Vector3.right * direccion * velocidad * Time.deltaTime);

            if (transform.position.x <= limiteMin) direccion = 1;
            else if (transform.position.x >= limiteMax) direccion = -1;
        }
        else // Vertical
        {
            // Movemos en el eje Y (Vector3.up)
            transform.Translate(Vector3.up * direccion * velocidad * Time.deltaTime);

            if (transform.position.y <= limiteMin) direccion = 1;
            else if (transform.position.y >= limiteMax) direccion = -1;
        }

        // Arrastrar al Player que va encima con el mismo desplazamiento.
        if (pasajero != null)
            pasajero.position += transform.position - antes;
    }

    // ── Detección del pasajero (funciona con collider sólido o trigger) ───
    private void OnCollisionEnter2D(Collision2D c) => TrySetPasajero(c.collider);
    private void OnCollisionExit2D(Collision2D c)  => TryClearPasajero(c.collider);
    private void OnTriggerEnter2D(Collider2D o)    => TrySetPasajero(o);
    private void OnTriggerExit2D(Collider2D o)     => TryClearPasajero(o);

    private void TrySetPasajero(Collider2D col)
    {
        // Solo si es el Player y va por ENCIMA de la plataforma.
        if (col.CompareTag("Player") && col.transform.position.y > transform.position.y)
            pasajero = col.transform;
    }

    private void TryClearPasajero(Collider2D col)
    {
        if (col.transform == pasajero) pasajero = null;
    }
}