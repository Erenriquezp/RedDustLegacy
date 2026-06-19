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

            // Si ya existe, no sobreescribir — protege cambios del Level Designer.
            if (AssetDatabase.LoadAssetAtPath<Tile>(fullPath) != null)
            {
                Debug.Log($"[PlaceholderTileGenerator] Ya existe, se omite: {assetName}");
                return;
            }

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.color = color;
            tile.sprite = GetOrCreatePlaceholderSprite(assetName, color);
            tile.colliderType = Tile.ColliderType.Sprite;

            AssetDatabase.CreateAsset(tile, fullPath);
        }

        /// <summary>
        /// Genera una textura 16×16 de color sólido y la guarda como sub-asset del Tile.
        /// No requiere ningún archivo de imagen externo.
        /// </summary>
        private static Sprite GetOrCreatePlaceholderSprite(string tileName, Color color)
        {
            const int SIZE = 16;
            const int PPU  = 16; // 1 unidad de mundo = 16 px → tile = 1×1 u

            Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false)
            {
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
