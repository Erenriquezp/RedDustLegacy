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
    [SerializeField] private AudioClip sfxScanStart; // NUEVO: Disparo inicial del escáner (Tecla E)
    [SerializeField] private AudioClip sfxScanStop;  // NUEVO: Feedback al apagar o terminar escaneo

    [Header("Ecualización y Volúmenes (Inspector)")]
    [Range(0f, 1f)][SerializeField] private float volIdle = 0.4f;
    [Range(0f, 1f)][SerializeField] private float volWalk = 0.5f;
    [Range(0f, 1f)][SerializeField] private float volJump = 0.6f;
    [Range(0f, 1f)][SerializeField] private float volDash = 0.6f;
    [Range(0f, 1f)][SerializeField] private float volLanding = 0.5f;
    [Range(0f, 1f)][SerializeField] private float volDamage = 0.7f;
    [Range(0f, 1f)][SerializeField] private float volScanEffects = 0.6f; // NUEVO

    [Header("Ajustes de Afinación (Pitch)")]
    [Range(0.5f, 1.5f)][SerializeField] private float pitchIdle = 1.0f;
    [Range(0.5f, 1.5f)][SerializeField] private float pitchWalk = 1.0f;

    [Header("Audio Sources Locales")]
    [SerializeField] private AudioSource loopAudioSource;   // Exclusivo para el motor (Idle/Walk)
    [SerializeField] private AudioSource oneShotAudioSource; // Exclusivo para impactos (Jump/Dash/Damage/Scan)

    private PlayerController _controller;
    private DegradationSystem _degradation;
    private RoverAudioState _currentState;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _degradation = GetComponent<DegradationSystem>();

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
            _controller.OnDashed += PlayDashSound;

            // NUEVOS EVENTOS: Escaneo del Rover (Tecla E)
            _controller.OnScanStarted += HandleScanStarted;
            _controller.OnScanStopped += HandleScanStopped;

            // S03 T1: feedback de daño/muerte desde DegradationSystem
            _controller.OnDamageReceived += HandleDamageReceived;
            _controller.OnDeath += PlayDeathSound;
            _controller.OnRevive += HandleRevive;   // T4: respawn
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

            // QUITAR NUEVOS EVENTOS
            _controller.OnScanStarted -= HandleScanStarted;
            _controller.OnScanStopped -= HandleScanStopped;

            _controller.OnDamageReceived -= HandleDamageReceived;
            _controller.OnDeath -= PlayDeathSound;
            _controller.OnRevive -= HandleRevive;
        }
    }

    private void Start()
    {
        _currentState = RoverAudioState.Idle;
        PlayEngineLoop(sfxIdle, volIdle, pitchIdle);
    }

    private void Update()
    {
        if (_controller == null || loopAudioSource == null) return;

        // ── NUEVO BLOQUEO DE SEGURIDAD ───────────────────────────────────────
        // Si el controlador está escaneando, salimos inmediatamente del Update.
        // Esto evita que la máquina de estados pise el bucle del escáner con el sonido Idle.
        if (_controller.IsScanning) return;

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

        if (targetState == _currentState) return;

        ChangeAudioState(_currentState, targetState);
        _currentState = targetState;
    }

    private void ChangeAudioState(RoverAudioState oldState, RoverAudioState newState)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        switch (newState)
        {
            case RoverAudioState.InAir:
                _fadeCoroutine = StartCoroutine(FadeEngineVolume(0f, 0.12f, stopAtEnd: true));
                break;

            case RoverAudioState.Scanning:
                // Al escanear el Rover se detiene, pasamos a un volumen de Idle muy sutil de fondo
                _fadeCoroutine = StartCoroutine(CrossfadeEngineClip(sfxIdle, volIdle * 0.4f, pitchIdle * 0.85f, 0.2f));
                break;

            case RoverAudioState.Idle:
                _fadeCoroutine = StartCoroutine(CrossfadeEngineClip(sfxIdle, volIdle, pitchIdle, 0.2f));
                break;

            case RoverAudioState.Walking:
                _fadeCoroutine = StartCoroutine(CrossfadeEngineClip(sfxWalk, volWalk, pitchWalk, 0.15f));
                break;
        }
    }

    // ── Callbacks de Escaneo (One-Shots Independientes) ──────────────────

    // ── Callbacks de Escaneo (Modificados para soportar Loop continuo) ────

    // ── Callbacks de Escaneo (Solución definitiva para evitar el conflicto con Update) ────

    // ── Callbacks de Escaneo (Asignando el loopAudioSource de forma segura) ────

    private void HandleScanStarted()
    {
        if (loopAudioSource != null && sfxScanStart != null)
        {
            // Cancelamos cualquier transición o desvanecimiento del motor en curso
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            // Configuramos el canal de bucle para el radar del escáner
            loopAudioSource.clip = sfxScanStart;
            loopAudioSource.volume = volScanEffects;
            loopAudioSource.pitch = 1.0f;
            loopAudioSource.loop = true; // ◄ Forzamos a Unity a que se repita en bucle

            if (!loopAudioSource.isPlaying) loopAudioSource.Play();
        }
    }

    private void HandleScanStopped()
    {
        if (loopAudioSource != null)
        {
            loopAudioSource.Stop();
            loopAudioSource.loop = true; // Devolvemos el comportamiento por defecto para cuando vuelva a Idle

            // Forzamos el estado de audio de vuelta a Idle para que el Update 
            // retome el sonido del motor fluido apenas sueltes la E
            _currentState = RoverAudioState.Idle;
            PlayEngineLoop(sfxIdle, volIdle, pitchIdle);
        }

        // Si tienes un sonido rápido "clic/desconexión" para el final, se dispara aquí en el One-Shot
        if (oneShotAudioSource != null && sfxScanStop != null)
        {
            oneShotAudioSource.pitch = 1.0f;
            oneShotAudioSource.PlayOneShot(sfxScanStop, volScanEffects);
        }
    }

    // ── Resto de implementaciones originales del script ─────────────────

    private IEnumerator CrossfadeEngineClip(AudioClip nextClip, float targetVolume, float targetPitch, float duration)
    {
        if (nextClip == null) yield break;

        float startVol = loopAudioSource.volume;
        float elapsed = 0f;
        while (elapsed < 0.05f)
        {
            elapsed += Time.deltaTime;
            loopAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / 0.05f);
            yield return null;
        }

        loopAudioSource.clip = nextClip;
        loopAudioSource.pitch = targetPitch;
        if (!loopAudioSource.isPlaying) loopAudioSource.Play();

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
        if (isGrounded && oneShotAudioSource != null && sfxLanding != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.95f, 1.05f);
            oneShotAudioSource.PlayOneShot(sfxLanding, volLanding);
        }
    }

    private void HandleRevive()
    {
        _currentState = RoverAudioState.Idle;
        PlayEngineLoop(sfxIdle, volIdle, pitchIdle);
    }

    public void PlayJumpSound()
    {
        if (oneShotAudioSource != null && sfxJump != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.93f, 1.05f);
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

    private float _lastDamageSfxTime = -1f;
    private const float DamageSfxCooldown = 0.4f;

    private void HandleDamageReceived(float amount)
    {
        if (Time.time - _lastDamageSfxTime < DamageSfxCooldown) return;
        _lastDamageSfxTime = Time.time;
        int fase = _degradation != null ? Mathf.Max(0, _degradation.CurrentPhase - 1) : 0;
        PlayDamageSound(fase);
    }

    public void PlayDamageSound(int faseDegradacion)
    {
        if (oneShotAudioSource != null && sfxDamage != null)
        {
            float nuevoPitch = 1.0f - (faseDegradacion * 0.05f);
            oneShotAudioSource.pitch = Mathf.Clamp(nuevoPitch, 0.65f, 1.0f);
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