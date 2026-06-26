using System.Collections;
using UnityEngine;

public class EnemyAudioController : MonoBehaviour
{
    [Header("Bucle de Presencia / Movimiento")]
    [SerializeField] private AudioClip sfxIdleLoop;
    [Range(0f, 1f)] [SerializeField] private float volIdle = 0.5f;

    [Header("Efectos de Acción (One-Shots)")]
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxDamage;
    [SerializeField] private AudioClip sfxDeath;
    
    [Range(0f, 1f)] [SerializeField] private float volActions = 0.6f;

    [Header("Fuentes de Audio Locales")]
    [SerializeField] private AudioSource loopAudioSource;   // Para el zumbido/respiración constante
    [SerializeField] private AudioSource oneShotAudioSource; // Para ataques, recibir daño y muerte

    private void Awake()
    {
        // Configuraciones automáticas de seguridad para sonido 3D
        ConfigureAudioSource(loopAudioSource, isLoop: true);
        ConfigureAudioSource(oneShotAudioSource, isLoop: false);
    }

    private void Start()
    {
        // En cuanto el enemigo aparece en el mapa, empieza a emitir su sonido de presencia
        PlayIdleLoop();
    }

    private void ConfigureAudioSource(AudioSource source, bool isLoop)
    {
        if (source == null) return;
        source.loop = isLoop;
        source.playOnAwake = false;
        
        // ¡Mucha atención aquí! Forzamos que el sonido sea 3D espacializado
        source.spatialBlend = 1.0f; 
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 2f;  // Se escucha a volumen máximo si el Rover está muy cerca
        source.maxDistance = 15f; // A los 15 metros de distancia ya no se escucha al enemigo
    }

    public void PlayIdleLoop()
    {
        if (loopAudioSource != null && sfxIdleLoop != null)
        {
            loopAudioSource.clip = sfxIdleLoop;
            loopAudioSource.volume = volIdle;
            loopAudioSource.Play();
        }
    }

    // --- Métodos públicos que llamará el programador de IA (feature/enemy-ai) ---

    public void PlayAttackSound()
    {
        if (oneShotAudioSource != null && sfxAttack != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.95f, 1.05f); // Variación orgánica
            oneShotAudioSource.PlayOneShot(sfxAttack, volActions);
        }
    }

    public void PlayDamageSound()
    {
        if (oneShotAudioSource != null && sfxDamage != null)
        {
            oneShotAudioSource.pitch = Random.Range(0.9f, 1.1f);
            oneShotAudioSource.PlayOneShot(sfxDamage, volActions);
        }
    }

    public void PlayDeathSound()
    {
        // Apagamos el bucle de presencia inmediatamente para que no siga "vivo" tras morir
        if (loopAudioSource != null) loopAudioSource.Stop();

        if (oneShotAudioSource != null && sfxDeath != null)
        {
            // Forzamos el pitch normal para la muerte para que sea dramático e identificable
            oneShotAudioSource.pitch = 1.0f; 
            oneShotAudioSource.PlayOneShot(sfxDeath, volActions);
        }
    }
}