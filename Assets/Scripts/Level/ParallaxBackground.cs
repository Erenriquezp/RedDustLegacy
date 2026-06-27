using UnityEngine;

/// <summary>
/// Parallax de fondo por desplazamiento de textura, pensado para cámara ORTOGRÁFICA 2D.
///
/// El contenedor (este GameObject) sigue a la cámara en X e Y para quedar SIEMPRE centrado
/// en pantalla; cada capa hija desplaza su textura según su profundidad en Z para dar la
/// sensación de parallax (las capas lejanas se mueven menos que las cercanas).
///
/// Requisitos:
///  - Cada hijo debe tener un Renderer con un material cuya textura esté en modo Wrap = Repeat.
///  - La cámara principal debe tener el tag "MainCamera".
///
/// El [DefaultExecutionOrder] alto garantiza que este LateUpdate corra DESPUÉS del de
/// CinemachineBrain: así el fondo se centra sobre la posición YA actualizada de la cámara y
/// no queda un frame por detrás (lo que abría huecos en los bordes al saltar/caer rápido).
/// </summary>
[DefaultExecutionOrder(1000)]
public class ParallaxMovement : MonoBehaviour
{
    Transform cam;            // cámara principal
    Vector3 camStartPos;      // posición inicial de la cámara (origen del desplazamiento)

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;
    float farthestBack;

    [Tooltip("Intensidad global del parallax. Mayor = el fondo se desplaza más respecto a la cámara.")]
    [Range(0.01f, 1f)]
    public float parallaxSpeed = 0.1f;

    [Tooltip("Si está activo, el fondo también hace parallax vertical al moverse la cámara en Y.")]
    public bool verticalParallax = true;

    [Tooltip("Nombre de la propiedad de textura en el shader (legacy/Sprites = _MainTex, URP = _BaseMap).")]
    public string textureProperty = "_MainTex";

    [Tooltip("Orden de renderizado que se aplica a TODAS las capas del fondo. Debe ser menor que el " +
             "de cualquier tilemap del nivel (Front-Tilemap = -2) para que el parallax quede detrás de todo.")]
    public int sortingOrder = -10;

    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("ParallaxMovement: no hay cámara con tag MainCamera; el parallax queda inactivo.");
            enabled = false;
            return;
        }

        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            Renderer rend = backgrounds[i].GetComponent<Renderer>();
            mat[i] = rend.material;
            // El fondo siempre detrás del arte del nivel; el orden relativo entre capas lo
            // resuelve la distancia en Z (la capa más lejana se dibuja antes).
            rend.sortingOrder = sortingOrder;
        }

        BackSpeedCalculate(backCount);
    }

    // Asigna a cada capa una velocidad relativa: la más lejana (mayor Z respecto a la cámara)
    // se mueve menos; la más cercana, casi a la par de la cámara.
    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++)
        {
            float depth = backgrounds[i].transform.position.z - cam.position.z;
            if (depth > farthestBack) farthestBack = depth;
        }

        if (farthestBack <= 0f) farthestBack = 1f; // evita división por cero si todas comparten Z

        for (int i = 0; i < backCount; i++)
        {
            float depth = backgrounds[i].transform.position.z - cam.position.z;
            backSpeed[i] = 1f - depth / farthestBack;
        }
    }

#if UNITY_EDITOR
    // El MeshRenderer no expone "Order in Layer" en el Inspector, así que aplicamos el orden
    // también en el editor (al cargar o cambiar valores) para previsualizar el fondo detrás
    // del nivel sin necesidad de entrar a Play. Solo toca el sortingOrder (no instancia material).
    void OnValidate()
    {
        foreach (Transform child in transform)
        {
            Renderer rend = child.GetComponent<Renderer>();
            if (rend != null) rend.sortingOrder = sortingOrder;
        }
    }
#endif

    void LateUpdate()
    {
        // El contenedor sigue a la cámara (X e Y) para mantenerse centrado en pantalla;
        // se conserva la Z inicial del contenedor (no se fuerza ninguna profundidad mágica).
        transform.position = new Vector3(cam.position.x, cam.position.y, transform.position.z);

        Vector2 dist = new Vector2(
            cam.position.x - camStartPos.x,
            verticalParallax ? cam.position.y - camStartPos.y : 0f);

        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset(textureProperty, dist * speed);
        }
    }
}
