using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controlador del menú principal (S04 T1, HUD §10.1) para MenuPrincipal.unity.
/// Localiza los botones por nombre y les cablea callbacks + SFX de UI en runtime,
/// para no depender de referencias de Inspector mientras el Artist itera la escena.
/// Nombres esperados: Btn_Continuar, Btn_NuevaMision, Btn_ArchivoDeMision,
/// Btn_Opciones, Btn_Sailr (sic — se acepta también Btn_Salir).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    private UIAudioController _uiAudio;

    private void Start()
    {
        _uiAudio = FindFirstObjectByType<UIAudioController>(FindObjectsInactive.Include);
        var buttons = CollectButtons();

        // CONTINUAR: placeholder — sin sistema de guardado aún, carga Level01 (spec T1.1).
        Button first = Wire(buttons, "Btn_Continuar", OnPlayPressed);
        Wire(buttons, "Btn_NuevaMision", OnPlayPressed);
        Wire(buttons, "Btn_Sailr", OnQuitPressed, "Btn_Salir");

        // Sin contenido todavía: Log Screen es V2 y el panel de Opciones no existe aún.
        Disable(buttons, "Btn_ArchivoDeMision");
        Disable(buttons, "Btn_Opciones");

        // Navegación con teclado/gamepad: dejar una opción seleccionada de entrada.
        if (first == null) first = Find(buttons, "Btn_NuevaMision");
        if (first != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(first.gameObject);
    }

    public void OnPlayPressed()
    {
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadLevel01();
        else Debug.LogError("[MainMenu] No hay SceneLoader en la escena.");
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Cableado por nombre ───────────────────────────────────────────────

    private static Dictionary<string, Button> CollectButtons()
    {
        var dict = new Dictionary<string, Button>();
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            dict[b.gameObject.name] = b;
        return dict;
    }

    private static Button Find(Dictionary<string, Button> buttons, string name, params string[] aliases)
    {
        if (buttons.TryGetValue(name, out var btn)) return btn;
        foreach (var alias in aliases)
            if (buttons.TryGetValue(alias, out btn)) return btn;
        return null;
    }

    private Button Wire(Dictionary<string, Button> buttons, string name,
                        UnityEngine.Events.UnityAction action, params string[] aliases)
    {
        var btn = Find(buttons, name, aliases);
        if (btn == null)
        {
            Debug.LogWarning($"[MainMenu] No se encontró el botón '{name}' en la escena.");
            return null;
        }
        if (_uiAudio != null) btn.onClick.AddListener(_uiAudio.PlayClickSound);
        btn.onClick.AddListener(action);
        AddHoverSound(btn);
        return btn;
    }

    private void Disable(Dictionary<string, Button> buttons, string name)
    {
        var btn = Find(buttons, name);
        if (btn != null) btn.interactable = false;
    }

    /// <summary>SFX de hover al pasar el ratón o al seleccionar con teclado/gamepad.</summary>
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
