// Assets/Scripts/Editor/InterludeScaffolder.cs
// SOLO para el Editor — NO se incluye en builds.
//
// S06 T1 — Prepara Isometric.unity como interludio N1→N2:
//   - Instancia el prefab HUD_Canvas si falta (la pausa necesita UI; sin
//     DegradationSystem el HUDManager oculta solo la barra de SI y las celdas).
//   - Crea el EventSystem (Input System) si falta — los botones de pausa no
//     reciben input sin él.
//   - Crea un trigger InterludeExit placeholder si falta. El LD debe MOVERLO
//     a la entrada del Relicto (queda junto al Player para que se vea).
//
// USO:
//   Unity Menu → Tools → Red Dust → Preparar interludio (Isometric)
//
// Idempotente: no duplica nada que ya exista.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class InterludeScaffolder
    {
        private const string ScenePath = "Assets/Scenes/Isometric/Isometric.unity";
        private const string HudPrefabPath = "Assets/Prefabs/HUD_Canvas/HUD_Canvas.prefab";

        [MenuItem("Tools/Red Dust/Preparar interludio (Isometric)")]
        public static void Scaffold()
        {
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = false;
            if (!scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
                openedHere = true;
            }

            bool dirty = false;

            // 1. HUD (pausa). El prefab ya trae PausePanel + PauseMenuController.
            if (FindInScene<HUDManager>(scene) == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"[InterludeScaffolder] No existe el prefab {HudPrefabPath}.");
                }
                else
                {
                    var hud = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                    Undo.RegisterCreatedObjectUndo(hud, "Preparar interludio");
                    dirty = true;
                }
            }

            // 2. EventSystem para los botones de pausa.
            if (FindInScene<EventSystem>(scene) == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                SceneManager.MoveGameObjectToScene(es, scene);
                Undo.RegisterCreatedObjectUndo(es, "Preparar interludio");
                dirty = true;
            }

            // 3. Trigger de salida (placeholder junto al Player; el LD lo coloca
            //    en la entrada del Relicto al montar el recorrido).
            if (FindInScene<InterludeExit>(scene) == null)
            {
                var player = FindInScene<Common.Scripts.BasicCharacter>(scene);
                Vector3 pos = player != null
                    ? player.transform.position + new Vector3(0f, 0f, 20f)
                    : Vector3.zero;

                var exit = new GameObject("InterludeExit (mover a la entrada del Relicto)",
                    typeof(BoxCollider), typeof(InterludeExit));
                exit.transform.position = pos;
                var box = exit.GetComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(12f, 12f, 3f);
                SceneManager.MoveGameObjectToScene(exit, scene);
                Undo.RegisterCreatedObjectUndo(exit, "Preparar interludio");
                dirty = true;
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[InterludeScaffolder] Isometric preparada: HUD de pausa, EventSystem y " +
                          "trigger de salida listos. Falta (LD): mover el InterludeExit a la entrada " +
                          "del Relicto, confiner de cámara, colliders del recorrido y viento (bus Ambient).");
            }
            else
            {
                Debug.Log("[InterludeScaffolder] Isometric ya estaba preparada; sin cambios.");
            }

            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<T>(true);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
