// Assets/Scripts/Editor/Level01Scaffolder.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Crea la jerarquía Grid + 4 tilemaps de Level01 con todos sus
// componentes ya configurados, para que el Level Designer solo
// tenga que PINTAR (ver Docs/Sprints/T5_Geometria_Level01.md).
//
// USO:
//   Unity Menu → Tools → Red Dust → Scaffold Level01 Grid
//
// RESULTADO (en la escena abierta):
//   Grid
//   ├── Visual_Tilemap      (Layer Default,  Order -10, sin collider)
//   ├── Collision_Tilemap   (Layer Ground,   Order  0,  TilemapCollider2D + CompositeCollider2D + Rigidbody2D Static)
//   ├── OneWay_Tilemap      (Layer Platform, Order  1,  + PlatformEffector2D, una sola dirección)
//   └── Markers_Tilemap     (Layer Default,  Order  5,  sin collider)

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public static class Level01Scaffolder
    {
        private const string GRID_NAME = "Grid";

        [MenuItem("Tools/Red Dust/Scaffold Level01 Grid")]
        public static void ScaffoldGrid()
        {
            // Protección: no duplicar si ya existe un Grid en la escena.
            var existing = Object.FindFirstObjectByType<Grid>();
            if (existing != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Grid existente",
                    $"Ya hay un '{existing.gameObject.name}' con Grid en la escena.\n\n" +
                    "Crear otro puede duplicar colisiones. ¿Continuar de todos modos?",
                    "Crear otro", "Cancelar");
                if (!overwrite) return;
            }

            // ── Grid raíz ─────────────────────────────────────────────
            var gridGO = new GameObject(GRID_NAME);
            Undo.RegisterCreatedObjectUndo(gridGO, "Scaffold Level01 Grid");
            var grid = gridGO.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f); // 1 tile = 1 unidad (16 PPU)

            // ── Visual_Tilemap — decoración, sin colisión ─────────────
            CreateTilemap(gridGO, "Visual_Tilemap", "Default", sortingOrder: -10);

            // ── Collision_Tilemap — suelo/paredes/techo ───────────────
            var collision = CreateTilemap(gridGO, "Collision_Tilemap", "Ground", sortingOrder: 0);
            var rb = collision.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var colCollider = collision.AddComponent<TilemapCollider2D>();
            colCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            collision.AddComponent<CompositeCollider2D>();

            // ── OneWay_Tilemap — plataformas unidireccionales ─────────
            var oneWay = CreateTilemap(gridGO, "OneWay_Tilemap", "Platform", sortingOrder: 1);
            var rbOne = oneWay.AddComponent<Rigidbody2D>();
            rbOne.bodyType = RigidbodyType2D.Static;
            var oneCollider = oneWay.AddComponent<TilemapCollider2D>();
            oneCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var oneComposite = oneWay.AddComponent<CompositeCollider2D>();
            oneComposite.usedByEffector = true;
            var effector = oneWay.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 170f; // colisión solo desde arriba

            // ── Markers_Tilemap — Tile_Blocked / Tile_Danger (visual) ─
            CreateTilemap(gridGO, "Markers_Tilemap", "Default", sortingOrder: 5);

            // Marcar la escena como modificada para que pida guardar.
            EditorSceneManager.MarkSceneDirty(gridGO.scene);

            Selection.activeGameObject = gridGO;
            EditorGUIUtility.PingObject(gridGO);
            Debug.Log("[Level01Scaffolder] Grid + 4 tilemaps creados. " +
                      "Abre Window → 2D → Tile Palette y empieza a pintar. " +
                      "Recuerda elegir el 'Active Tilemap' correcto al pintar.");
        }

        // ─────────────────────────────────────────────────────────────

        /// <summary>Crea un hijo del Grid con Tilemap + TilemapRenderer ya listos.</summary>
        private static GameObject CreateTilemap(GameObject grid, string name, string layerName, int sortingOrder)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Scaffold Level01 Grid");
            go.transform.SetParent(grid.transform, worldPositionStays: false);

            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
                Debug.LogWarning($"[Level01Scaffolder] El layer '{layerName}' no existe; '{name}' queda en Default.");
            else
                go.layer = layer;

            go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            return go;
        }
    }
}
#endif
