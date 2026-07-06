using UnityEngine;
using UnityEngine.SceneManagement; // ¡IMPORTANTE! Requerido para cambiar de escena

public class MenuInicio : MonoBehaviour
{
    // Este método lo llamará el botón "Jugar" o "Iniciar"
    public void JugarVideojuego()
    {
        // Cambia a la escena del primer nivel. 
        // Asegúrate de que el nombre entre comillas sea EXACTAMENTE igual al de tu escena.
        SceneManager.LoadScene("Mundo1");
    }

    // Este método lo llamará el botón "Salir"
    public void SalirDelJuego()
    {
        Debug.Log("El jugador ha salido del videojuego.");
        Application.Quit(); // Cierra el juego (funciona en el juego exportado .exe/.apk)
    }
}