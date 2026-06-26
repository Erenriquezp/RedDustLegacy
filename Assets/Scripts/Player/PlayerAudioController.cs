using System.Collections;
using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    private enum RoverAudioState { InAir, Idle, Walking, Scanning }

    [Header("Efectos de Sonido del Rover")]
    [SerializeField] private AudioClip sfxIdle;
    [SerializeField] private AudioClip sfxWalk;
    [SerializeField] private AudioClip sfxJump;
    [SerializeField] private AudioClip sfxDash;      // T4.5: Sonido exclusivo para el Dash
    [SerializeField] private AudioClip sfxLanding;   // Añadido para suavizar las caídas
    [SerializeField] private AudioClip sfxDamage;
    [SerializeField] private AudioClip sfxDeath;

    [Header("Ecualización y Volúmenes (Inspector)")]
    [Range(0f, 1f)] [SerializeField] private float volIdle = 0.4f;
    [Range(0f, 1f)] [SerializeField] private float volWalk = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float volJump = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float volDash = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float volLanding = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float volDamage = 0.7f;

    [Header("Ajustes de Afinación (Pitch)")]
    [Range(0.5f, 1.5f)] [SerializeField] private float pitchIdle = 1.0f;
    [Range(0.5f, 1.5f)] [SerializeField] private float pitchWalk = 1.0f;

    [Header("Audio Sources Locales")]
    [SerializeField] private AudioSource loopAudioSource;   // Exclusivo para el motor (Idle/Walk)
    [SerializeField] private AudioSource oneShotAudioSource; // Exclusivo para impactos (Jump/Dash/Damage)

    private PlayerController _controller;
    private RoverAudioState _currentState;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        
        // Configuración inicial automática de seguridad
        if (loopAudioSource != null)
        {
            loopAudioSource.loop = true;
            loopAudioSource.playOnAwake = false;
        }
        if (oneShotAudioSource != null)
        {
            oneShotAudioSource.loop = false;
            oneShotAudioSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        if (_controller != null)
        {
            _controller.OnGroundedChanged += HandleGroundedChanged;
            _controller.OnJumped += PlayJumpSound;
            _controller.OnWallJumped += PlayJumpSound;
            
            // T4.5: ¡Bug de S01 solucionado! OnDashed mapeado a su propio sonido de Dash
            _controller.OnDashed += PlayDashSound; 
        }
    }

    private void OnDisable()
    {
        if (_controller != null)
        {
            _controller.OnGroundedChanged -= HandleGroundedChanged;
            _controller.OnJumped -= PlayJumpSound;
            _controller.OnWallJumped -= PlayJumpSound;
            _controller.OnDashed -= PlayDashSound;
        }
    }

    private void Start()
    {
        // Estado inicial
        _currentState = RoverAudioState.Idle;
        PlayEngineLoop(sfxIdle, volIdle, pitchIdle);
    }

    private void Update()
    {
        if (_controller == null || loopAudioSource == null) return;

        // Máquina de estados: Calculamos de forma exacta qué está haciendo el Rover en este frame
        RoverAudioState targetState = RoverAudioState.Idle;

        if (!_controller.IsGrounded)
        {
            targetState = RoverAudioState.InAir;
        }
        else if (_controller.IsScanning)
        {
            targetState = RoverAudioState.Scanning;
        }
        else if (Mathf.Abs(_controller.GetMoveInput()) > 0.01f)
        {
            targetState = RoverAudioState.Walking;
        }

        // Si el estado no ha cambiado, no tocamos nada (evita reinicios raros de audio)
        if (targetState == _currentState) return;

        // Ejecutar la transición al nuevo estado
        ChangeAudioState(_currentState, targetState);
        _currentState = targetState;
    }

    private void ChangeAudioState(RoverAudioState oldState, RoverAudioState newState)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        switch (newState)
        {
            case RoverAudioState.InAir:
                // Si salta o cae, desvanecemos el motor rápido para que no flote el sonido de ruedas
                _fadeCoroutine = StartCoroutine(FadeEngineVolume(0f, 0.12f, stopAtEnd: true));
                break;

            case RoverAudioState.Scanning:
                // Al escanear el Rover se detiene (según tu PlayerController), pasamos a volumen de Idle bajo
                _fadeCoroutine = StartCoroutine(CrossfadeEngineClip(sfxIdle, volIdle * 0.5f, pitchIdle, 0.2f));
                break;

            case RoverAudioState.Idle:
                // Volvemos a reposo fluido
                _fadeCoroutine = StartCoroutine(CrossfadeEngineClip(sfxIdle, volIdle, pitchIdle, 0.2f));
                break;

            case RoverAudioState.Walking:
                // Transición al sonido de movimiento en ruedas
                _fadeCoroutine = StartCoroutine(CrossfadeEngineClip(sfxWalk, volWalk, pitchWalk, 0.15f));
                break;
        }
    }

    private IEnumerator CrossfadeEngineClip(AudioClip nextClip, float targetVolume, float targetPitch, float duration)
    {
        if (nextClip == null) yield break;

        // Desvanecimiento rápido del volumen actual para evitar clics/pops mecánicos
        float startVol = loopAudioSource.volume;
        float elapsed = 0f;
        while (elapsed < 0.05f)
        {
            elapsed += Time.deltaTime;
            loopAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / 0.05f);
            yield return null;
        }

        // Hacemos el cambio de clip en silencio absoluto
        loopAudioSource.clip = nextClip;
        loopAudioSource.pitch = targetPitch;
        if (!loopAudioSource.isPlaying) loopAudioSource.Play();

        // Subimos el volumen suavemente hasta el nivel deseado de este sonido específico
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            loopAudioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        loopAudioSource.volume = targetVolume;
    }

    private IEnumerator FadeEngineVolume(float targetVolume, float duration, bool stopAtEnd)
    {
        float startVol = loopAudioSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            loopAudioSource.volume = Mathf.Lerp(startVol, targetVolume, elapsed / duration);
            yield return null;
        }

        loopAudioSource.volume = targetVolume;
        if (stopAtEnd) loopAudioSource.Stop();
    }

    private void PlayEngineLoop(AudioClip clip, float volume, float pitch)
    {
        if (loopAudioSource == null || clip == null) return;
        loopAudioSource.clip = clip;
        loopAudioSource.volume = volume;
        loopAudioSource.pitch = pitch;
        loopAudioSource.Play();
    }

    private void HandleGroundedChanged(bool isGrounded)
    {
        // Efecto de aterrizaje para dar feedback de peso al Rover
        if (isGrounded && oneShotAudioSource != null && sfxLanding != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.95f, 1.05f);
            oneShotAudioSource.PlayOneShot(sfxLanding, volLanding);
        }
    }

    // ── Disparos de efectos One-Shot (Canal secundario independiente) ──

    public void PlayJumpSound()
    {
        if (oneShotAudioSource != null && sfxJump != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.93f, 1.05f); // Variación orgánica para que no aburra
            oneShotAudioSource.PlayOneShot(sfxJump, volJump);
        }
    }

    public void PlayDashSound()
    {
        if (oneShotAudioSource != null && sfxDash != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.97f, 1.03f);
            oneShotAudioSource.PlayOneShot(sfxDash, volDash);
        }
    }

    // T4.5: Modificado para recibir la degradación y aplicar pitch adaptativo
    public void PlayDamageSound(int faseDegradacion)
    {
        if (oneShotAudioSource != null && sfxDamage != null)
        {
            // El sprint exige: pitch -5% por cada fase de degradación activa (-0.05f por fase)
            float nuevoPitch = 1.0f - (faseDegradacion * 0.05f);
            oneShotAudioSource.pitch = Mathf.Clamp(nuevoPitch, 0.65f, 1.0f); // Límite seguro para no distorsionar feo
            
            oneShotAudioSource.PlayOneShot(sfxDamage, volDamage);
        }
    }

    public void PlayDeathSound()
    {
        if (loopAudioSource != null) loopAudioSource.Stop(); 
        if (oneShotAudioSource != null && sfxDeath != null)
        {
            oneShotAudioSource.pitch = 1.0f;
            oneShotAudioSource.PlayOneShot(sfxDeath, 1.0f);
        }
    }
}