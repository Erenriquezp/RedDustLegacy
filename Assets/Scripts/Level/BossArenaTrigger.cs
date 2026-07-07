using UnityEngine;

/// <summary>
/// Arena del boss (S04 T2/T3, HUD §4.1.2 punto 7). Colocar como trigger en la
/// entrada de la arena: al pasar el rover cierra el lockdown y arranca el boss
/// (que debe tener <c>startDormant</c> activo). Al morir el boss abre la salida.
/// Requiere un Collider2D con isTrigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BossArenaTrigger : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private LeviatanAI boss;

    [Header("Lockdown")]
    [Tooltip("Barrera que cierra la arena al entrar (muro/puerta). Se apaga al morir el boss.")]
    [SerializeField] private GameObject barrier;

    [Header("Al derrotar al boss")]
    [Tooltip("Se activa al morir el boss: LevelExit / trigger de la cinemática de caída (S04 T2 punto 8).")]
    [SerializeField] private GameObject exitToUnlock;

    private bool _triggered;

    private void Awake()
    {
        if (barrier != null) barrier.SetActive(false);
        if (exitToUnlock != null) exitToUnlock.SetActive(false);
    }

    private void OnEnable()
    {
        if (boss != null) boss.OnDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        if (boss != null) boss.OnDefeated -= HandleBossDefeated;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !other.CompareTag("Player")) return;
        _triggered = true;

        if (barrier != null) barrier.SetActive(true);
        if (boss != null) boss.StartEncounter();
    }

    private void HandleBossDefeated()
    {
        if (barrier != null) barrier.SetActive(false);
        if (exitToUnlock != null) exitToUnlock.SetActive(true);
    }
}
