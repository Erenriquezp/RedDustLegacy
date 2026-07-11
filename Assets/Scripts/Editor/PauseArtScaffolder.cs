// Assets/Scripts/Editor/PauseArtScaffolder.cs
// SOLO para el Editor — NO se incluye en builds.
//
// S04 T1.2 — Arte de pausa: copia el layout del Canvas de StopMenu.unity
// (la vista aislada del Artist) al PausePanel del prefab HUD_Canvas,
// sustituyendo el placeholder del HudScaffolder, y añade PauseMenuController
// al panel (cablea los botones por nombre en runtime, así que no hay
// listeners persistentes que crear).
//
// USO:
//   Unity Menu → Tools → Red Dust → Copiar arte de StopMenu al PausePanel
//
// Idempotente: re-ejecutable tras retocar StopMenu — borra el contenido
// anterior del PausePanel y vuelve a copiar.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class PauseArtScaffolder
    {
        private const string StopMenuPath  = "Assets/Scenes/MainMenu/StopMenu.unity";
        private const string HudPrefabPath = "Assets/Prefabs/HUD_Canvas/HUD_Canvas.prefab";

        [MenuItem("Tools/Red Dust/Copiar arte de StopMenu al PausePanel")]
        public static void CopyPauseArt()
        {
            // 1. Asegurar StopMenu cargado (aditivo solo si no estaba ya abierto,
            //    para no cerrarle la escena al usuario al terminar).
            Scene stopMenu = SceneManager.GetSceneByPath(StopMenuPath);
            bool openedHere = false;
            if (!stopMenu.isLoaded)
            {
                stopMenu = EditorSceneManager.OpenScene(StopMenuPath, OpenSceneMode.Additive);
                openedHere = true;
            }

            try
            {
                Canvas canvas = FindCanvas(stopMenu);
                if (canvas == null)
                {
                    EditorUtility.DisplayDialog("StopMenu sin Canvas",
                        $"No se encontró un Canvas en {StopMenuPath}.", "Cerrar");
                    return;
                }

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(HudPrefabPath);
                try
                {
                    Transform pausePanel = prefabRoot.transform.Find("PausePanel");
                    if (pausePanel == null)
                    {
                        EditorUtility.DisplayDialog("PausePanel no encontrado",
                            $"El prefab {HudPrefabPath} no tiene un hijo 'PausePanel'.", "Cerrar");
                        return;
                    }

                    // 2. Sustituir el placeholder por el arte real. Solo se copian
                    //    los hijos del Canvas (los objetos raíz de StopMenu — cámara,
                    //    EventSystem, BGM, SceneLoader, partículas — no son del layout).
                    for (int i = pausePanel.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(pausePanel.GetChild(i).gameObject);

                    int copied = 0;
                    foreach (Transform child in canvas.transform)
                    {
                        GameObject copy = Object.Instantiate(child.gameObject, pausePanel);
                        copy.name = child.gameObject.name; // sin sufijo "(Clone)"
                        copied++;
                    }

                    // 3. El controlador vive en el propio panel: se cablea por nombre
                    //    de botón cuando GameManager lo activa por primera vez.
                    if (pausePanel.GetComponent<PauseMenuController>() == null)
                        pausePanel.gameObject.AddComponent<PauseMenuController>();

                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, HudPrefabPath);
                    Debug.Log($"[PauseArtScaffolder] {copied} elementos del Canvas de StopMenu " +
                              "copiados al PausePanel de HUD_Canvas y PauseMenuController añadido. " +
                              "Verifica la pausa en Play mode (Level01).");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
            finally
            {
                if (openedHere) EditorSceneManager.CloseScene(stopMenu, true);
            }
        }

        private static Canvas FindCanvas(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                var canvas = root.GetComponentInChildren<Canvas>(true);
                if (canvas != null) return canvas;
            }
            return null;
        }
    }
}
#endif
