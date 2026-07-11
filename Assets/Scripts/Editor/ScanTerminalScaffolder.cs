// Assets/Scripts/Editor/ScanTerminalScaffolder.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Construye el panel ScanTerminal (S05 T1) dentro del HUD_Canvas existente y lo
// deja CABLEADO al componente ScanTerminalUI: fondo #060610, línea de acento de
// severidad, encabezado y cuerpo TMP. Guía: Docs/Architecture/ScanSystem.md.
//
// USO:
//   1. Abrir el prefab HUD_Canvas (doble clic) — o una escena que lo contenga.
//   2. Unity Menu → Tools → Red Dust → Scaffold Scan Terminal.
//   3. Guardar el prefab. (Recomendado: asignar una fuente TMP monoespaciada
//      a Header/Body — la estética es de terminal.)

#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    public static class ScanTerminalScaffolder
    {
        [MenuItem("Tools/Red Dust/Scaffold Scan Terminal")]
        public static void Scaffold()
        {
            var hud = Object.FindFirstObjectByType<HUDManager>(FindObjectsInactive.Include);
            if (hud == null)
            {
                EditorUtility.DisplayDialog("Sin HUD",
                    "No hay un HUDManager abierto. Abre el prefab HUD_Canvas (o una escena que lo contenga) y vuelve a ejecutar.",
                    "OK");
                return;
            }

            var existing = hud.GetComponentInChildren<ScanTerminalUI>(true);
            if (existing != null)
            {
                EditorGUIUtility.PingObject(existing);
                EditorUtility.DisplayDialog("Ya existe",
                    "Este HUD_Canvas ya tiene un ScanTerminal. Bórralo primero si quieres regenerarlo.", "OK");
                return;
            }

            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Panel raíz (abajo-derecha, GDD §11.1: no tapa al rover ni la barra de SI) ──
            RectTransform panel = CreateRect("ScanTerminal", hud.transform,
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            panel.sizeDelta = new Vector2(760f, 230f);
            panel.anchoredPosition = new Vector2(-40f, 40f);
            Undo.RegisterCreatedObjectUndo(panel.gameObject, "Scaffold Scan Terminal");

            var group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            // Fondo terminal (#060610 al 88%)
            Image bg = AddImage(CreateRect("Background", panel, stretch: true), uiSprite,
                new Color(0.023f, 0.023f, 0.063f, 0.88f));
            bg.type = Image.Type.Sliced;

            // Línea de acento superior (3 px) — toma el color de severidad en runtime
            RectTransform accentRT = CreateRect("Accent", panel,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            accentRT.sizeDelta = new Vector2(0f, 3f);
            accentRT.anchoredPosition = Vector2.zero;
            Image accent = AddImage(accentRT, uiSprite, Color.white);

            // Encabezado (ej. "ANALISIS — SC-01")
            RectTransform headerRT = CreateRect("Header", panel,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
            headerRT.sizeDelta = new Vector2(-40f, 40f);
            headerRT.anchoredPosition = new Vector2(0f, -14f);
            TMP_Text header = AddTMP(headerRT, "ANALISIS — SC-00", 26f, TextAlignmentOptions.TopLeft);
            header.fontStyle = FontStyles.Bold;

            // Cuerpo (typewriter)
            RectTransform bodyRT = CreateRect("Body", panel, stretch: true);
            bodyRT.offsetMin = new Vector2(20f, 16f);
            bodyRT.offsetMax = new Vector2(-20f, -58f);
            TMP_Text body = AddTMP(bodyRT, "", 22f, TextAlignmentOptions.TopLeft);

            // ── Componente + cableado ─────────────────────────────────────
            var terminal = panel.gameObject.AddComponent<ScanTerminalUI>();
            var so = new SerializedObject(terminal);
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("headerText").objectReferenceValue = header;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("accent").objectReferenceValue = accent;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
            Selection.activeGameObject = panel.gameObject;
            EditorGUIUtility.PingObject(panel.gameObject);

            Debug.Log("[ScanTerminalScaffolder] ScanTerminal creado y cableado bajo " +
                      $"{hud.name}. Recomendado: fuente TMP monoespaciada en Header/Body. " +
                      "Si estás en el prefab HUD_Canvas, guárdalo (Ctrl+S).");
        }

        // ── Helpers (mismo patrón que HudScaffolder) ──────────────────────

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
            img.raycastTarget = false;
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
    }
}
#endif
