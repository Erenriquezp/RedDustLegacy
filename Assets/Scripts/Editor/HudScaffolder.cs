// Assets/Scripts/Editor/HudScaffolder.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Construye el Canvas del HUD de Level01 (Sprint 03 T2) con todos sus elementos
// ya creados y CABLEADOS al componente HUDManager, para no montarlo a mano:
//   - Barra de Integridad Estructural (Image tipo Filled + texto TMP) con raíz de pulso
//   - 2 slots de celda
//   - Alert Strip (CanvasGroup + fondo + texto TMP)
//   - GameOverPanel (desactivado)
//
// USO:
//   Unity Menu → Tools → Red Dust → Scaffold HUD Canvas
//
// RESULTADO (en la escena abierta): un "HUD_Canvas" (Screen Space Overlay,
// referencia 1920×1080) con HUDManager y sus referencias asignadas. Si hay un
// DegradationSystem en la escena, también lo enlaza.

#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Editor
{
    public static class HudScaffolder
    {
        private const string CANVAS_NAME = "HUD_Canvas";

        [MenuItem("Tools/Red Dust/Scaffold HUD Canvas")]
        public static void ScaffoldHud()
        {
            var existing = Object.FindObjectsByType<HUDManager>(FindObjectsSortMode.None);
            if (existing.Length > 0)
            {
                bool cont = EditorUtility.DisplayDialog(
                    "HUD existente",
                    "Ya hay un HUDManager en la escena. Crear otro puede duplicar el HUD. ¿Continuar?",
                    "Crear otro", "Cancelar");
                if (!cont) { EditorGUIUtility.PingObject(existing[0]); return; }
            }

            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Canvas raíz ───────────────────────────────────────────────
            var canvasGO = new GameObject(CANVAS_NAME,
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(HUDManager));
            Undo.RegisterCreatedObjectUndo(canvasGO, "Scaffold HUD Canvas");

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var hud = canvasGO.GetComponent<HUDManager>();

            // ── Barra de SI (raíz de pulso, abajo-izquierda) ──────────────
            RectTransform siBar = CreateRect("SIBar", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            siBar.sizeDelta = new Vector2(420f, 48f);
            siBar.anchoredPosition = new Vector2(40f, 48f);

            // Fondo
            Image barBg = AddImage(CreateRect("Background", siBar, stretch: true), uiSprite,
                new Color(0f, 0f, 0f, 0.55f));
            barBg.type = Image.Type.Sliced;

            // Relleno (Filled horizontal) — lo dirige HUDManager
            RectTransform fillRT = CreateRect("Fill", siBar, stretch: true);
            fillRT.offsetMin = new Vector2(3f, 3f);
            fillRT.offsetMax = new Vector2(-3f, -3f);
            Image fill = AddImage(fillRT, uiSprite, new Color(0.298f, 0.686f, 0.314f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0.74f;

            // Texto de SI
            TMP_Text siText = AddTMP(CreateRect("SIText", siBar, stretch: true), "SI 74%",
                26f, TextAlignmentOptions.Center);
            siText.fontStyle = FontStyles.Bold;

            // ── Slots de celda (encima de la barra) ───────────────────────
            RectTransform cellsRoot = CreateRect("Cells", canvasGO.transform,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            cellsRoot.sizeDelta = new Vector2(120f, 40f);
            cellsRoot.anchoredPosition = new Vector2(40f, 104f);

            Image cell0 = MakeCellSlot("CellSlot_0", cellsRoot, uiSprite, 0f);
            Image cell1 = MakeCellSlot("CellSlot_1", cellsRoot, uiSprite, 44f);

            // ── Alert Strip (arriba-centro) ───────────────────────────────
            RectTransform alert = CreateRect("AlertStrip", canvasGO.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            alert.sizeDelta = new Vector2(720f, 64f);
            alert.anchoredPosition = new Vector2(0f, -90f);
            var alertGroup = alert.gameObject.AddComponent<CanvasGroup>();
            alertGroup.alpha = 0f;

            AddImage(CreateRect("AlertBG", alert, stretch: true), uiSprite,
                new Color(0f, 0f, 0f, 0.6f)).type = Image.Type.Sliced;
            TMP_Text alertText = AddTMP(CreateRect("AlertText", alert, stretch: true),
                "ALERTA", 30f, TextAlignmentOptions.Center);
            alertText.fontStyle = FontStyles.Bold;

            // ── Relay de escena para los botones (enruta al GameManager) ──
            var actions = canvasGO.AddComponent<GameMenuActions>();

            // ── PausePanel (pantalla completa, desactivado) ───────────────
            RectTransform pausePanel = CreateRect("PausePanel", canvasGO.transform, stretch: true);
            AddImage(pausePanel, uiSprite, new Color(0f, 0f, 0f, 0.75f)).type = Image.Type.Sliced;
            RectTransform pauseTitle = CreateRect("PauseTitle", pausePanel,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            pauseTitle.sizeDelta = new Vector2(900f, 90f);
            pauseTitle.anchoredPosition = new Vector2(0f, 200f);
            AddTMP(pauseTitle, "DIAGNÓSTICO EN PAUSA", 48f, TextAlignmentOptions.Center);

            Button bResume  = CreateButton("Btn_Resume",  pausePanel, "REANUDAR DIAGNÓSTICO",       uiSprite, new Vector2(0f,  60f));
            Button bRestart = CreateButton("Btn_Restart", pausePanel, "REINICIAR DESDE CHECKPOINT", uiSprite, new Vector2(0f, -20f));
            Button bMenu    = CreateButton("Btn_Menu",    pausePanel, "ABANDONAR SESIÓN",           uiSprite, new Vector2(0f, -100f));
            pausePanel.gameObject.SetActive(false);

            // ── GameOverPanel (pantalla completa, desactivado) ────────────
            RectTransform gameOver = CreateRect("GameOverPanel", canvasGO.transform, stretch: true);
            AddImage(gameOver, uiSprite, new Color(0f, 0f, 0f, 0.85f)).type = Image.Type.Sliced;
            RectTransform goTitle = CreateRect("GameOverText", gameOver,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            goTitle.sizeDelta = new Vector2(900f, 120f);
            goTitle.anchoredPosition = new Vector2(0f, 120f);
            AddTMP(goTitle, "SISTEMA INOPERATIVO", 60f, TextAlignmentOptions.Center)
                .color = new Color(0.86f, 0.18f, 0.16f);

            Button goRestart = CreateButton("Btn_GO_Restart", gameOver, "REINTENTAR",      uiSprite, new Vector2(0f, -40f));
            Button goMenu    = CreateButton("Btn_GO_Menu",    gameOver, "MENÚ PRINCIPAL",  uiSprite, new Vector2(0f, -120f));
            gameOver.gameObject.SetActive(false);

            // ── EventSystem (necesario para que los botones reciban input) ─
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Scaffold HUD Canvas");
            }

            // ── Cablear onClick de los botones al relay (listeners persistentes) ─
            UnityEventTools.AddPersistentListener(bResume.onClick,   actions.Resume);
            UnityEventTools.AddPersistentListener(bRestart.onClick,  actions.Restart);
            UnityEventTools.AddPersistentListener(bMenu.onClick,     actions.ToMainMenu);
            UnityEventTools.AddPersistentListener(goRestart.onClick, actions.Restart);
            UnityEventTools.AddPersistentListener(goMenu.onClick,    actions.ToMainMenu);

            // ── Cablear referencias del HUDManager (campos privados) ──────
            var so = new SerializedObject(hud);
            SetRef(so, "_pausePanel", pausePanel.gameObject);
            SetRef(so, "_degradation", Object.FindFirstObjectByType<DegradationSystem>());
            SetRef(so, "_siFill", fill);
            SetRef(so, "_siText", siText);
            SetRef(so, "_siPulseRoot", siBar);
            SetRef(so, "_alertGroup", alertGroup);
            SetRef(so, "_alertText", alertText);
            SetRef(so, "_gameOverPanel", gameOver.gameObject);

            var cells = so.FindProperty("_cellSlots");
            cells.arraySize = 2;
            cells.GetArrayElementAtIndex(0).objectReferenceValue = cell0;
            cells.GetArrayElementAtIndex(1).objectReferenceValue = cell1;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(canvasGO.scene);
            Selection.activeGameObject = canvasGO;
            EditorGUIUtility.PingObject(canvasGO);

            bool linked = Object.FindFirstObjectByType<DegradationSystem>() != null;
            Debug.Log($"[HudScaffolder] HUD_Canvas creado y cableado. DegradationSystem " +
                      (linked ? "enlazado." : "NO encontrado (el HUD lo resolverá en runtime si lo añades al Player).") +
                      " Ajusta tipografía/estética; el layout es funcional pero básico.");
        }

        // ─────────────────────────────────────────────────────────────────

        private static Image MakeCellSlot(string name, Transform parent, Sprite sprite, float x)
        {
            RectTransform rt = CreateRect(name, parent,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            rt.sizeDelta = new Vector2(36f, 36f);
            rt.anchoredPosition = new Vector2(18f + x, 0f);
            // Vacío por defecto (gris 40%); HUDManager lo actualiza.
            Image img = AddImage(rt, sprite, new Color(1f, 1f, 1f, 0.4f));
            img.type = Image.Type.Sliced;
            return img;
        }

        private static Button CreateButton(string name, Transform parent, string label,
            Sprite sprite, Vector2 anchoredPos)
        {
            RectTransform rt = CreateRect(name, parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.sizeDelta = new Vector2(420f, 64f);
            rt.anchoredPosition = anchoredPos;

            Image img = AddImage(rt, sprite, new Color(0.10f, 0.45f, 0.65f, 0.92f));
            img.type = Image.Type.Sliced;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            TMP_Text label_ = AddTMP(CreateRect("Label", rt, stretch: true), label,
                24f, TextAlignmentOptions.Center);
            label_.fontStyle = FontStyles.Bold;

            return btn;
        }

        private static RectTransform CreateRect(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, worldPositionStays: false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            return rt;
        }

        private static RectTransform CreateRect(string name, Transform parent, bool stretch)
        {
            var rt = CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            if (stretch)
            {
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            return rt;
        }

        private static Image AddImage(RectTransform rt, Sprite sprite, Color color)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return img;
        }

        private static TMP_Text AddTMP(RectTransform rt, string text, float size, TextAlignmentOptions align)
        {
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void SetRef(SerializedObject so, string prop, Object value)
        {
            var p = so.FindProperty(prop);
            if (p != null) p.objectReferenceValue = value;
        }
    }
}
#endif
