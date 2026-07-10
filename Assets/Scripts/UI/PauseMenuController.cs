using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Cablea los botones de la pantalla de pausa (S04 T1, HUD §10.3) por nombre y
/// los enruta al <see cref="GameManager"/>. Funciona tanto en StopMenu.unity
/// (vista aislada) como colocado dentro del PausePanel del prefab HUD_Canvas.
/// Nombres esperados: Btn_ReanudarDiagnostico, Btn_ReiniciarCheckPoint,
/// Btn_Configuracion, Btn_Archivo, Btn_AbandonarSesion.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    private UIAudioController _uiAudio;
    private Button _first;

    private void Start()
    {
        _uiAudio = FindFirstObjectByType<UIAudioController>(FindObjectsInactive.Include);
        var buttons = CollectButtons();

        _first = Wire(buttons, "Btn_ReanudarDiagnostico", Resume);
        Wire(buttons, "Btn_ReiniciarCheckPoint", Restart);
        Wire(buttons, "Btn_AbandonarSesion", ToMainMenu);

        // Sin contenido todavía: Log Screen es V2 y el panel de Opciones no existe aún.
        Disable(buttons, "Btn_Configuracion");
        Disable(buttons, "Btn_Archivo");

        SelectFirst();
    }

    /// <summary>Al reabrir la pausa (panel desactivado/activado), re-seleccionar para teclado/gamepad.</summary>
    private void OnEnable() => SelectFirst();

    private void SelectFirst()
    {
        if (_first != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_first.gameObject);
    }

    // ── Callbacks (mismo enrutado que GameMenuActions) ────────────────────

    private void Resume()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    private void Restart()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartFromCheckpoint();
    }

    private void ToMainMenu()
    {
        if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadMainMenu();
    }

    // ── Cableado por nombre ───────────────────────────────────────────────

    /// <summary>
    /// Si este componente vive dentro del panel de pausa, busca solo entre sus hijos
    /// (para no capturar botones del Game Over u otros paneles del HUD);
    /// si está suelto en la escena (StopMenu), busca en toda la escena.
    /// </summary>
    private Dictionary<string, Button> CollectButtons()
    {
        var own = GetComponentsInChildren<Button>(true);
        var source = own.Length > 0
            ? own
            : FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        var dict = new Dictionary<string, Button>();
        foreach (var b in source) dict[b.gameObject.name] = b;
        return dict;
    }

    private Button Wire(Dictionary<string, Button> buttons, string name,
                        UnityEngine.Events.UnityAction action)
    {
        if (!buttons.TryGetValue(name, out var btn))
        {
            Debug.LogWarning($"[PauseMenu] No se encontró el botón '{name}'.");
            return null;
        }
        if (_uiAudio != null) btn.onClick.AddListener(_uiAudio.PlayClickSound);
        btn.onClick.AddListener(action);
        AddHoverSound(btn);
        return btn;
    }

    private void Disable(Dictionary<string, Button> buttons, string name)
    {
        if (buttons.TryGetValue(name, out var btn)) btn.interactable = false;
    }

    private void AddHoverSound(Button btn)
    {
        if (_uiAudio == null) return;
        var trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

        var hover = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hover.callback.AddListener(_ => _uiAudio.PlayHoverSound());
        trigger.triggers.Add(hover);

        var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        select.callback.AddListener(_ => _uiAudio.PlayHoverSound());
        trigger.triggers.Add(select);
    }
}
