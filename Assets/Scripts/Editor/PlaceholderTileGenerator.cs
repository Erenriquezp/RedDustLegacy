// Assets/Scripts/Editor/PlaceholderTileGenerator.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Genera los Tile Assets de placeholder que el Level Designer
// necesita ANTES de tener los sprites finales del artista.
//
// USO:
//   Unity Menu → Tools → Red Dust → Generate Placeholder Tiles
//
// OUTPUT:
//   Assets/Art/Tiles/Placeholders/
//   ├── Tile_Ground_Placeholder.asset        (gris oscuro)
//   ├── Tile_Platform_Placeholder.asset      (gris claro)
//   ├── Tile_Blocked_Placeholder.asset       (rojo — zona Rueda Reforzada)
//   ├── Tile_Background_Placeholder.asset    (azul oscuro — visual sin colisión)
//   └── Tile_Danger_Placeholder.asset        (naranja — daño / foso)

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Editor
{
    public static class PlaceholderTileGenerator
    {
        private const string OUTPUT_PATH = "Assets/Art/Tiles/Placeholders";

        [MenuItem("Tools/Red Dust/Generate Placeholder Tiles")]
        public static void GenerateAll()
        {
            EnsureDirectory(OUTPUT_PATH);

            CreateTile("Tile_Ground_Placeholder",      new Color(0.35f, 0.35f, 0.40f)); // gris oscuro
            CreateTile("Tile_Platform_Placeholder",    new Color(0.60f, 0.58f, 0.55f)); // gris claro
            CreateTile("Tile_Blocked_Placeholder",     new Color(0.85f, 0.15f, 0.15f)); // rojo bloqueado
            CreateTile("Tile_Background_Placeholder",  new Color(0.08f, 0.10f, 0.22f)); // azul fondo
            CreateTile("Tile_Danger_Placeholder",      new Color(1.00f, 0.50f, 0.00f)); // naranja peligro

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PlaceholderTileGenerator] Tiles generados en {OUTPUT_PATH}");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(OUTPUT_PATH);
        }

        // ─────────────────────────────────────────────────────────────

        private static void CreateTile(string assetName, Color color)
        {
            string fullPath = $"{OUTPUT_PATH}/{assetName}.asset";

            Tile existing = AssetDatabase.LoadAssetAtPath<Tile>(fullPath);
            if (existing != null)
            {
                // Si ya tiene sprite, está sano → no lo tocamos (protege ajustes del Level Designer).
                if (existing.sprite != null)
                {
                    Debug.Log($"[PlaceholderTileGenerator] Ya existe y tiene sprite, se omite: {assetName}");
                    return;
                }

                // Reparar tiles generados por la versión con bug (sprite no persistido → null).
                existing.color = color;
                existing.colliderType = Tile.ColliderType.Sprite;
                existing.sprite = CreateAndEmbedSprite(assetName, color, existing);
                EditorUtility.SetDirty(existing);
                Debug.Log($"[PlaceholderTileGenerator] Reparado (sprite faltante): {assetName}");
                return;
            }

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.color = color;
            tile.colliderType = Tile.ColliderType.Sprite;

            // El Tile debe existir en disco ANTES de incrustarle sub-assets.
            AssetDatabase.CreateAsset(tile, fullPath);
            tile.sprite = CreateAndEmbedSprite(assetName, color, tile);
            EditorUtility.SetDirty(tile);
        }

        /// <summary>
        /// Genera una textura 16×16 de color sólido y la incrusta —junto con su Sprite—
        /// como sub-assets del Tile <paramref name="owner"/>, de modo que la referencia
        /// persiste al recargar el proyecto. No requiere ningún archivo de imagen externo.
        /// </summary>
        private static Sprite CreateAndEmbedSprite(string tileName, Color color, Object owner)
        {
            const int SIZE = 16;
            const int PPU  = 16; // 1 unidad de mundo = 16 px → tile = 1×1 u

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false)
            {
                name       = $"{tileName}_Tex",
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };

            // Relleno sólido con borde ligeramente más oscuro (2 px)
            Color borderColor = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, 1f);
            Color fillColor   = new Color(color.r,         color.g,         color.b,         1f);

            for (int y = 0; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
            {
                bool isBorder = x < 2 || x >= SIZE - 2 || y < 2 || y >= SIZE - 2;
                tex.SetPixel(x, y, isBorder ? borderColor : fillColor);
            }
            tex.Apply();

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, SIZE, SIZE),
                new Vector2(0.5f, 0.5f),
                PPU
            );
            sprite.name = $"{tileName}_Sprite";

            // Persistir textura y sprite como sub-assets del Tile (esto faltaba).
            AssetDatabase.AddObjectToAsset(tex, owner);
            AssetDatabase.AddObjectToAsset(sprite, owner);

            return sprite;
        }

        private static void EnsureDirectory(string path)
        {
            // Crea recursivamente: Assets → Art → Tiles → Placeholders
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
