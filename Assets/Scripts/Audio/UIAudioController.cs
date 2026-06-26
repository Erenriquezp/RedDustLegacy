using UnityEngine;

public class UIAudioController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Efectos de Sonido de Interfaz")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    // Esta función la llamaremos cuando el cursor pase por encima
    public void PlayHoverSound()
    {
        if (sfxAudioSource != null && hoverClip != null)
        {
            sfxAudioSource.PlayOneShot(hoverClip);
        }
    }

    // Esta función la llamaremos cuando hagan clic
    public void PlayClickSound()
    {
        if (sfxAudioSource != null && clickClip != null)
        {
            sfxAudioSource.PlayOneShot(clickClip);
        }
    }
}