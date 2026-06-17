using UnityEngine;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources Globales")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxGlobalSource;

        [Header("Pistas de Música (BGM)")]
        [SerializeField] private AudioClip bgmMainTheme;
        [SerializeField] private AudioClip bgmGameOver;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // El tema principal inicia en loop automáticamente en el Gameplay
            PlayMusic(bgmMainTheme, true);
        }

        public void PlayMusic(AudioClip clip, bool makeLoop)
        {
            if (musicSource == null || clip == null) return;
            
            musicSource.clip = clip;
            musicSource.loop = makeLoop;
            musicSource.Play();
        }

        public void TriggerGameOverMusic()
        {
            // Game Over no lleva loop (según especificación)
            PlayMusic(bgmGameOver, false);
        }

        public void PlayGlobalSFX(AudioClip clip)
        {
            if (sfxGlobalSource != null && clip != null)
            {
                sfxGlobalSource.PlayOneShot(clip);
            }
        }
    }
}