using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer y Ruteo")]
        [SerializeField] private AudioMixer mainMixer;

        [Header("Fuentes de Audio Globales")]
        [SerializeField] private AudioSource musicSourceA; // Este es tu antiguo musicSource
        [SerializeField] private AudioSource musicSourceB; // El nuevo para hacer el crossfade
        [SerializeField] private AudioSource sfxGlobalSource;

        [Header("Pistas de Música Adaptativa (S02/S04)")]
        [SerializeField] private AudioClip clipExploration; // Tu antiguo bgmMainTheme va aquí
        [SerializeField] private AudioClip clipTension;
        [SerializeField] private AudioClip clipCombat;
        [SerializeField] private AudioClip clipCinematic;
        [SerializeField] private AudioClip bgmGameOver; // Lo mantenemos intacto

        // Requisito del Sprint 02
        public enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }
        private MusicState _currentState = MusicState.Silence;

        private AudioSource _activeSource;
        private AudioSource _inactiveSource;
        private Coroutine _crossfadeRoutine;

        private void Awake()
        {
            // Como ahora es por nivel, solo asignamos la instancia. 
            // Si por algún motivo se recarga la escena y hay dos, destruimos el viejo.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // ¡ADIÓS DontDestroyOnLoad! Ahora este DJ vive y muere en su propio nivel.

            _activeSource = musicSourceA;
            _inactiveSource = musicSourceB;
        }

        private void Start()
        {
            // Arranca la música base de exploración automáticamente (como tu viejo Start)
            SetMusicState(MusicState.Exploration);
        }

        // --------------------------------------------------------
        // 1. CONTROL DE VOLUMEN MAESTRO (Mixer)
        // --------------------------------------------------------

        public void SetMusicVolume(float v01) => SetMixerVolume("MusicVol", v01);
        public void SetSFXVolume(float v01) => SetMixerVolume("SfxVol", v01);
        public void SetAmbientVolume(float v01) => SetMixerVolume("AmbientVol", v01);
        public void SetUIVolume(float v01) => SetMixerVolume("UiVol", v01);

        private void SetMixerVolume(string exposedName, float v01)
        {
            if (mainMixer == null) return;
            float dB = Mathf.Log10(Mathf.Clamp(v01, 0.0001f, 1f)) * 20f;
            mainMixer.SetFloat(exposedName, dB);
        }

        // --------------------------------------------------------
        // 2. SISTEMA ADAPTATIVO (Crossfade)
        // --------------------------------------------------------

        public void SetMusicState(MusicState newState)
        {
            if (newState == _currentState) return;

            AudioClip nextClip = null;
            float fadeTime = 1.0f;

            switch (newState)
            {
                case MusicState.Exploration: nextClip = clipExploration; fadeTime = 1.5f; break;
                case MusicState.Tension: nextClip = clipTension; fadeTime = 0.8f; break;
                case MusicState.Combat: nextClip = clipCombat; fadeTime = 0.5f; break;
                case MusicState.Cinematic: nextClip = clipCinematic; fadeTime = 1.0f; break;
                case MusicState.Silence: nextClip = null; fadeTime = 1.0f; break;
            }

            _currentState = newState;

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = StartCoroutine(Crossfade(nextClip, fadeTime));
        }

        private IEnumerator Crossfade(AudioClip nextClip, float duration)
        {
            AudioSource temp = _activeSource;
            _activeSource = _inactiveSource;
            _inactiveSource = temp;

            _activeSource.clip = nextClip;
            _activeSource.volume = 0f;
            if (nextClip != null) _activeSource.Play();

            float time = 0f;
            float startInactiveVol = _inactiveSource.volume;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                _activeSource.volume = Mathf.Lerp(0f, 1f, t);
                _inactiveSource.volume = Mathf.Lerp(startInactiveVol, 0f, t);
                yield return null;
            }

            _activeSource.volume = 1f;
            _inactiveSource.Stop();
            _inactiveSource.volume = 0f;
        }

        // --------------------------------------------------------
        // 3. RETROCOMPATIBILIDAD (Tu código antiguo intacto)
        // --------------------------------------------------------

        public void PlayMusic(AudioClip clip, bool makeLoop)
        {
            // Si otra clase llama a esto, interrumpimos el crossfade adaptativo
            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            
            _currentState = MusicState.Silence; 

            _activeSource.clip = clip;
            _activeSource.loop = makeLoop;
            _activeSource.volume = 1f;
            _activeSource.Play();

            if (_inactiveSource != null)
            {
                _inactiveSource.Stop();
                _inactiveSource.volume = 0f;
            }
        }

        public void TriggerGameOverMusic()
        {
            // Usa tu función clásica exactamente como la tenías
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