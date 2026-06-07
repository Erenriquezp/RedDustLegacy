using UnityEngine;
using System.Collections;
using Core;

public class PlayerAudioController : MonoBehaviour
{
    [Header("Efectos de Sonido del Rover")]
    [SerializeField] private AudioClip sfxIdle;
    [SerializeField] private AudioClip sfxWalk;
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxDamage;
    [SerializeField] private AudioClip sfxDeath;

    [Header("Audio Sources Locales (Instanciados en el Rover)")]
    [SerializeField] private AudioSource loopAudioSource;   // Bocina principal para el motor
    [SerializeField] private AudioSource oneShotAudioSource; // Bocina secundaria (se usará para One-Shots y transiciones)

    private PlayerController _controller;
    private bool _wasMoving = false;
    private Coroutine _fadeCoroutine;
    private float _originalLoopVolume;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        if (loopAudioSource != null)
        {
            // Guardamos el volumen original que configuraste en el Inspector (ej: 1.0 o 0.8)
            _originalLoopVolume = loopAudioSource.volume;
        }
    }

    private void OnEnable()
    {
        if (_controller != null)
        {
            _controller.OnGroundedChanged += HandleGroundedChanged;
            _controller.OnJumped += PlayJumpSound;
            _controller.OnDashed += PlayDamageSound; 
            _controller.OnWallJumped += PlayJumpSound;
        }
    }

    private void OnDisable()
    {
        if (_controller != null)
        {
            _controller.OnGroundedChanged -= HandleGroundedChanged;
            _controller.OnJumped -= PlayJumpSound;
            _controller.OnDashed -= PlayDamageSound;
            _controller.OnWallJumped -= PlayJumpSound;
        }
    }

    private void Update()
    {
        if (_controller == null || loopAudioSource == null) return;

        // Si despega del suelo, apagamos el motor rápido con un Fade corto
        if (!_controller.IsGrounded)
        {
            if (loopAudioSource.isPlaying && _fadeCoroutine == null)
            {
                _fadeCoroutine = StartCoroutine(FadeOutLoopOnly(0.15f));
                _wasMoving = false;
            }
            return;
        }

        // Detectamos si el Rover se está moviendo en el suelo
        bool isMovingNow = Mathf.Abs(_controller.GetMoveInput()) > 0.01f;

        // Si cambia el estado (de quieto a moviéndose, o viceversa) iniciamos la transición suave
        if (isMovingNow != _wasMoving || !loopAudioSource.isPlaying)
        {
            _wasMoving = isMovingNow;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            
            AudioClip nextClip = isMovingNow ? sfxWalk : sfxIdle;
            _fadeCoroutine = StartCoroutine(CrossfadeEngine(nextClip, 0.2f)); // 0.2 segundos de fundido
        }
    }

    // Corrutina que hace la magia del fundido cruzado para que el paso de Idle a Walk sea imperceptible
    private IEnumerator CrossfadeEngine(AudioClip targetClip, float duration)
    {
        if (targetClip == null) yield break;

        // Si la bocina ya estaba tocando el clip correcto, no hacemos nada
        if (loopAudioSource.isPlaying && loopAudioSource.clip == targetClip)
        {
            loopAudioSource.volume = _originalLoopVolume;
            yield break;
        }

        // Si la bocina estaba en silencio o apagada, simplemente subimos el volumen suavemente
        if (!loopAudioSource.isPlaying)
        {
            loopAudioSource.clip = targetClip;
            loopAudioSource.volume = 0f;
            loopAudioSource.loop = true;
            loopAudioSource.Play();

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                loopAudioSource.volume = Mathf.Lerp(0f, _originalLoopVolume, t / duration);
                yield return null;
            }
            loopAudioSource.volume = _originalLoopVolume;
            yield break;
        }

        // --- AQUÍ SE SOLUCIONA TU PROBLEMA ---
        // Usamos la bocina secundaria temporalmente para que el sonido viejo se apague suavizándose, 
        // mientras la bocina principal arranca el nuevo sonido desde volumen cero.
        oneShotAudioSource.clip = loopAudioSource.clip;
        oneShotAudioSource.time = loopAudioSource.time; // Sincroniza el punto exacto de la reproducción
        oneShotAudioSource.volume = loopAudioSource.volume;
        oneShotAudioSource.loop = true;
        oneShotAudioSource.Play();

        // Cambiamos la bocina principal al nuevo archivo con volumen 0
        loopAudioSource.clip = targetClip;
        loopAudioSource.volume = 0f;
        loopAudioSource.Play();

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / duration;

            // La bocina principal SUBE de volumen, la bocina secundaria BAJA de volumen
            loopAudioSource.volume = Mathf.Lerp(0f, _originalLoopVolume, normalizedTime);
            oneShotAudioSource.volume = Mathf.Lerp(_originalLoopVolume, 0f, normalizedTime);

            yield return null;
        }

        // Aseguramos los volúmenes finales y apagamos la bocina temporal
        loopAudioSource.volume = _originalLoopVolume;
        oneShotAudioSource.Stop();
        oneShotAudioSource.volume = _originalLoopVolume; // Lo restauramos para los sonidos de saltos

        _fadeCoroutine = null;
    }

    private IEnumerator FadeOutLoopOnly(float duration)
    {
        float startVol = loopAudioSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            loopAudioSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        loopAudioSource.Stop();
        loopAudioSource.volume = _originalLoopVolume;
        _fadeCoroutine = null;
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        if (!isGrounded && loopAudioSource.isPlaying)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutLoopOnly(0.15f));
        }
    }

    // --- Métodos públicos de disparo rápido (One-Shots) ---
    public void PlayJumpSound()
    {
        if (oneShotAudioSource != null && sfxJump != null)
        {
            oneShotAudioSource.PlayOneShot(sfxJump);
        }
    }

    public void PlayDamageSound()
    {
        if (oneShotAudioSource != null && sfxDamage != null)
        {
            oneShotAudioSource.PlayOneShot(sfxDamage);
        }
    }

    public void PlayDeathSound()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        if (loopAudioSource != null) loopAudioSource.Stop(); 

        if (oneShotAudioSource != null && sfxDeath != null)
        {
            oneShotAudioSource.PlayOneShot(sfxDeath);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.TriggerGameOverMusic();
        }
    }
}