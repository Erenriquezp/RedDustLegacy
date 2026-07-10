using System.Collections;
using System.Collections.Generic;
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

        // Variables de memoria para recordar los decibelios exactos de tu mezcla en el editor
        private float _originalMusicdB;
        private float _originalSfxdB;
        private float _originalAmbientdB;
        private float _originalUidB;

        // Requisito del Sprint 02
        public enum MusicState { Silence, Exploration, Tension, Combat, Cinematic }
        private MusicState _currentState = MusicState.Silence;

        private AudioSource _activeSource;
        private AudioSource _inactiveSource;
        private Coroutine _crossfadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // ── NUEVO: LEER Y GUARDAR TU MEZCLA DEL EDITOR ───────────────────
            if (mainMixer != null)
            {
                // Leemos los dB exactos que configuraste en el Main Mixer para cada canal
                mainMixer.GetFloat("MusicVol", out _originalMusicdB);
                mainMixer.GetFloat("SfxVol", out _originalSfxdB);
                mainMixer.GetFloat("AmbientVol", out _originalAmbientdB);
                mainMixer.GetFloat("UiVol", out _originalUidB);
            }
            // ─────────────────────────────────────────────────────────────────

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

        // ── S04 T4.2: disparadores de Tension ────────────────────────────────
        // Contador central de enemigos en Alert/Chase. Con un solo bool por enemigo
        // la música bajaría a Exploration cuando UN enemigo pierde al rover aunque
        // otro siga persiguiendo; el set resuelve eso.
        private readonly HashSet<Object> _enemiesInAlert = new HashSet<Object>();

        /// <summary>
        /// Los enemigos (Biol/Drone) reportan aquí al entrar/salir de Alert-Chase.
        /// ≥1 en alerta → Tension; 0 → Exploration. Nunca pisa Combat/Cinematic
        /// (los gestiona el boss/las cinemáticas) ni suena fuera de gameplay.
        /// </summary>
        public void ReportEnemyAlert(Object source, bool inAlert)
        {
            if (inAlert) _enemiesInAlert.Add(source);
            else _enemiesInAlert.Remove(source);

            if (_currentState == MusicState.Combat || _currentState == MusicState.Cinematic)
                return;
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            SetMusicState(_enemiesInAlert.Count > 0 ? MusicState.Tension : MusicState.Exploration);
        }

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


        // ── NUEVO MÉTODO PÚBLICO PARA REINTEGROS / CHECKPOINTS ────────────────
        // Esta función la debe llamar el script del Checkpoint o el botón "Reintentar" 
        // cuando el jugador reaparece sin recargar la escena.
        public void RestartExplorationMusicOnCheckpoint()
        {
            Time.timeScale = 1f;

            // ── RESTAURACIÓN FIEL DE TU MEZCLA ORIGINAL ──────────────────────
            if (mainMixer != null)
            {
                // Devolvemos el Mixer EXACTAMENTE a como lo ecualizaste en el Inspector
                mainMixer.SetFloat("MusicVol", _originalMusicdB); 
                mainMixer.SetFloat("SfxVol", _originalSfxdB);
                mainMixer.SetFloat("AmbientVol", _originalAmbientdB);
                mainMixer.SetFloat("UiVol", _originalUidB);
            }
            // ─────────────────────────────────────────────────────────────────

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            if (_activeSource != null) _activeSource.Stop();
            if (_inactiveSource != null) _inactiveSource.Stop();

            _currentState = MusicState.Silence;

            if (_activeSource != null && clipExploration != null)
            {
                _currentState = MusicState.Exploration;
                _activeSource.clip = clipExploration;
                _activeSource.volume = 1f;
                _activeSource.loop = true;
                _activeSource.Play();
            }
        }
    }
    
}