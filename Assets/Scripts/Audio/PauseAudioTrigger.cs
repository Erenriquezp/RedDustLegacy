using UnityEngine;
using UnityEngine.Audio;

public class PauseAudioTrigger : MonoBehaviour
{
    [Header("Configuración del Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    private void OnEnable()
    {
        if (mainMixer != null)
        {
            // Bajamos los efectos del juego y ambiente a -80dB (silencio absoluto)
            mainMixer.SetFloat("SfxVol", -80f);
            mainMixer.SetFloat("AmbientVol", -80f);
        }
    }

    private void OnDisable()
    {
        if (mainMixer != null)
        {
            // Al regresar al juego, devolvemos el volumen a su estado normal (0dB)
            // Nota: Si tienen un menú de opciones que guarde el volumen, aquí deberías 
            // cargar el valor que el usuario eligió en vez de forzar un 0f.
            mainMixer.SetFloat("SfxVol", 0f);
            mainMixer.SetFloat("AmbientVol", 0f);
        }
    }
}