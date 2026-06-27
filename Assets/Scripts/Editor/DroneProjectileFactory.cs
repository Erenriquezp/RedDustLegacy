// Assets/Scripts/Editor/DroneProjectileFactory.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Genera el prefab del proyectil del Drone Patrullero (Sprint 03 T3) con todos
// sus componentes ya configurados, para no tener que armarlo a mano:
//   - SpriteRenderer (sprite placeholder redondo, visible de inmediato)
//   - Rigidbody2D Kinematic, gravity 0
//   - CircleCollider2D radio 0.2, Is Trigger ON
//   - EnemyProjectile
//   - Layer "EnemyProjectile"
//
// USO:
//   Unity Menu → Tools → Red Dust → Create DroneProjectile Prefab
//
// RESULTADO:
//   Assets/Prefabs/Enemies/DroneProjectile.prefab
//   Además, lo asigna a los DronePatrollerAI de la escena abierta que tengan
//   'projectilePrefab' vacío.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public static class DroneProjectileFactory
    {
        private const string FOLDER = "Assets/Prefabs/Enemies";
        private const string PREFAB_PATH = FOLDER + "/DroneProjectile.prefab";
        private const string LAYER_NAME = "EnemyProjectile";

        [MenuItem("Tools/Red Dust/Create DroneProjectile Prefab")]
        public static void CreateDroneProjectile()
        {
            // 1) No duplicar: si ya existe, preguntar antes de sobrescribir.
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            if (existing != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Prefab existente",
                    $"Ya existe '{PREFAB_PATH}'.\n¿Sobrescribirlo con la configuración por defecto?",
                    "Sobrescribir", "Cancelar");
                if (!overwrite)
                {
                    EditorGUIUtility.PingObject(existing);
                    return;
                }
            }

            EnsureFolder(FOLDER);

            // 2) Construir el GameObject temporal en memoria.
            var go = new GameObject("DroneProjectile");

            int layer = LayerMask.NameToLayer(LAYER_NAME);
            if (layer < 0)
                Debug.LogWarning($"[DroneProjectileFactory] El layer '{LAYER_NAME}' no existe; el prefab queda en Default. " +
                                 "Créalo en Project Settings → Tags and Layers.");
            else
                go.layer = layer;

            // SpriteRenderer con un sprite redondo de Unity (placeholder visible).
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(1f, 0.55f, 0.2f, 1f); // ámbar, contrasta con la cueva azul
            sr.sortingOrder = 10;

            // Rigidbody2D Kinematic sin gravedad (lo mueve EnemyProjectile por Transform).
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            // CircleCollider2D trigger.
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;

            // Lógica del proyectil.
            go.AddComponent<EnemyProjectile>();

            // Escala discreta para que la bola placeholder no sea enorme.
            go.transform.localScale = Vector3.one * 0.4f;

            // 3) Guardar como prefab y limpiar el temporal de la escena.
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PREFAB_PATH);
            Object.DestroyImmediate(go);

            if (prefab == null)
            {
                Debug.LogError("[DroneProjectileFactory] No se pudo crear el prefab.");
                return;
            }

            // 4) Conveniencia: asignarlo a los Drones de la escena que no tengan proyectil.
            int wired = AssignToSceneDrones(prefab);

            AssetDatabase.SaveAssets();
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"[DroneProjectileFactory] Prefab creado en {PREFAB_PATH}. " +
                      $"Asignado a {wired} DronePatrollerAI de la escena sin proyectil. " +
                      "Asigna un sprite definitivo cuando lo tengas.");
        }

        /// <summary>Asigna el prefab a los DronePatrollerAI abiertos cuyo projectilePrefab esté vacío.</summary>
        private static int AssignToSceneDrones(GameObject prefab)
        {
            int count = 0;
            var drones = Object.FindObjectsByType<DronePatrollerAI>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var ai in drones)
            {
                if (ai.projectilePrefab != null) continue;
                Undo.RecordObject(ai, "Assign DroneProjectile");
                ai.projectilePrefab = prefab;
                EditorUtility.SetDirty(ai);
                count++;
            }

            if (count > 0 && drones.Length > 0)
                EditorSceneManager.MarkSceneDirty(drones[0].gameObject.scene);

            return count;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
