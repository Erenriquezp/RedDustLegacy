using UnityEngine;

public class UIAudioController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Efectos de Sonido de Interfaz")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip backClip;
    [SerializeField] private AudioClip pauseOpenClip;
    [SerializeField] private AudioClip pauseCloseClip;
    [SerializeField] private AudioClip loadingTickClip;

    public void PlayHoverSound() { if (sfxAudioSource && hoverClip) sfxAudioSource.PlayOneShot(hoverClip); }
    public void PlayClickSound() { if (sfxAudioSource && clickClip) sfxAudioSource.PlayOneShot(clickClip); }
    
    public void PlayBackSound() { if (sfxAudioSource && backClip) sfxAudioSource.PlayOneShot(backClip); }
    public void PlayPauseOpenSound() { if (sfxAudioSource && pauseOpenClip) sfxAudioSource.PlayOneShot(pauseOpenClip); }
    public void PlayPauseCloseSound() { if (sfxAudioSource && pauseCloseClip) sfxAudioSource.PlayOneShot(pauseCloseClip); }
    public void PlayLoadingTickSound() { if (sfxAudioSource && loadingTickClip) sfxAudioSource.PlayOneShot(loadingTickClip); }
}