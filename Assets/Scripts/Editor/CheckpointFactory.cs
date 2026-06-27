// Assets/Scripts/Editor/CheckpointFactory.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Crea un Checkpoint ya configurado (BoxCollider2D trigger 3×4) y garantiza que
// exista un CheckpointManager en la escena — Sprint 03 T4.
//
// USO:
//   Unity Menu → Tools → Red Dust → Create Checkpoint

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class CheckpointFactory
    {
        [MenuItem("Tools/Red Dust/Create Checkpoint")]
        public static void CreateCheckpoint()
        {
            // 1) Garantizar el CheckpointManager de escena.
            if (Object.FindFirstObjectByType<CheckpointManager>() == null)
            {
                var mgr = new GameObject("CheckpointManager", typeof(CheckpointManager));
                Undo.RegisterCreatedObjectUndo(mgr, "Create CheckpointManager");
            }

            // 2) Crear el Checkpoint.
            var go = new GameObject("Checkpoint", typeof(Checkpoint));
            Undo.RegisterCreatedObjectUndo(go, "Create Checkpoint");

            var box = go.GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(3f, 4f);

            // Colocar en el pivote de la Scene View para verlo de inmediato.
            var view = SceneView.lastActiveSceneView;
            if (view != null) go.transform.position = view.pivot;

            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            Debug.Log("[CheckpointFactory] Checkpoint creado (trigger 3×4). Colócalo sobre el suelo; " +
                      "≥8 u del enemigo más cercano (GDD §7). Repite para los 3 checkpoints del nivel.");
        }
    }
}
#endif
