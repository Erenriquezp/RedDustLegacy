using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Level
{
    /// <summary>
    /// Marcador visual de Scene View para placeholders del Level01.
    /// NO requiere sprites. Dibuja geometría con Gizmos + label.
    ///
    /// USO: Añadir a un GameObject vacío. Configurar tipo en Inspector.
    /// En builds de producción este componente no genera overhead
    /// (toda la lógica vive en #if UNITY_EDITOR).
    ///
    /// TIPOS DISPONIBLES:
    ///   SpawnPoint        → Cruz verde  (posición de spawn del Player)
    ///   Checkpoint        → Bandera azul
    ///   Scannable         → Rombo cyan  (SC-01, SC-02, SC-03)
    ///   BlockedZone       → Cubo rojo   (zona Rueda Reforzada — 30% inaccesible)
    ///   EnemyPatrol       → Flecha naranja con rango
    ///   BossRoom          → Cubo magenta grande
    ///   EnergyCellPickup  → Estrella amarilla
    /// </summary>
    public class LevelMarker : MonoBehaviour
    {
        public enum MarkerType
        {
            SpawnPoint,
            Checkpoint,
            Scannable,
            BlockedZone,
            EnemyPatrol,
            BossRoom,
            EnergyCellPickup
        }

        [Header("Configuración")]
        public MarkerType type = MarkerType.SpawnPoint;

        [Tooltip("Etiqueta visible en Scene View (ej: 'SC-01', 'CP-Zona2', 'Spawn_Player').")]
        public string label = "";

        [Header("Blocked Zone / Enemy Patrol — Dimensiones")]
        [Tooltip("Solo para BlockedZone y EnemyPatrol: tamaño del área en unidades de mundo.")]
        public Vector2 areaSize = new Vector2(4f, 3f);

        // ─────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private static readonly Color _colorSpawn       = new Color(0.2f, 1f,   0.2f, 0.9f);
        private static readonly Color _colorCheckpoint  = new Color(0.3f, 0.6f, 1f,   0.9f);
        private static readonly Color _colorScannable   = new Color(0f,   1f,   1f,   0.9f);
        private static readonly Color _colorBlocked     = new Color(1f,   0.15f, 0.15f, 0.55f);
        private static readonly Color _colorBlockedWire = new Color(1f,   0.15f, 0.15f, 0.9f);
        private static readonly Color _colorPatrol      = new Color(1f,   0.55f, 0f,   0.9f);
        private static readonly Color _colorBoss        = new Color(0.9f, 0.1f,  0.9f, 0.55f);
        private static readonly Color _colorBossWire    = new Color(0.9f, 0.1f,  0.9f, 0.9f);
        private static readonly Color _colorEnergy      = new Color(1f,   0.9f,  0f,   0.9f);

        private void OnDrawGizmos()
        {
            Vector3 pos = transform.position;
            string displayLabel = string.IsNullOrEmpty(label) ? type.ToString() : $"{type} — {label}";

            switch (type)
            {
                case MarkerType.SpawnPoint:
                    DrawCross(pos, 0.5f, _colorSpawn);
                    DrawLabel(pos + Vector3.up * 0.7f, displayLabel, _colorSpawn);
                    break;

                case MarkerType.Checkpoint:
                    DrawFlag(pos, _colorCheckpoint);
                    DrawLabel(pos + Vector3.up * 1.2f, displayLabel, _colorCheckpoint);
                    break;

                case MarkerType.Scannable:
                    DrawDiamond(pos, 0.4f, _colorScannable);
                    DrawLabel(pos + Vector3.up * 0.7f, displayLabel, _colorScannable);
                    break;

                case MarkerType.BlockedZone:
                    Gizmos.color = _colorBlocked;
                    Gizmos.DrawCube(pos, new Vector3(areaSize.x, areaSize.y, 0.1f));
                    Gizmos.color = _colorBlockedWire;
                    Gizmos.DrawWireCube(pos, new Vector3(areaSize.x, areaSize.y, 0.1f));
                    DrawLabel(pos, $"🔒 {displayLabel}", _colorBlockedWire);
                    break;

                case MarkerType.EnemyPatrol:
                    DrawPatrolArrows(pos, areaSize.x, _colorPatrol);
                    DrawLabel(pos + Vector3.up * 0.8f, displayLabel, _colorPatrol);
                    break;

                case MarkerType.BossRoom:
                    Gizmos.color = _colorBoss;
                    Gizmos.DrawCube(pos, new Vector3(areaSize.x, areaSize.y, 0.1f));
                    Gizmos.color = _colorBossWire;
                    Gizmos.DrawWireCube(pos, new Vector3(areaSize.x, areaSize.y, 0.1f));
                    DrawLabel(pos, $"⚠ {displayLabel}", _colorBossWire);
                    break;

                case MarkerType.EnergyCellPickup:
                    DrawStar(pos, 0.35f, _colorEnergy);
                    DrawLabel(pos + Vector3.up * 0.65f, displayLabel, _colorEnergy);
                    break;
            }
        }

        // ── Helpers de dibujo ────────────────────────────────────────

        private static void DrawCross(Vector3 pos, float size, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawLine(pos + Vector3.left  * size, pos + Vector3.right * size);
            Gizmos.DrawLine(pos + Vector3.down  * size, pos + Vector3.up    * size);
            Gizmos.DrawWireSphere(pos, size * 0.25f);
        }

        private static void DrawFlag(Vector3 pos, Color color)
        {
            Gizmos.color = color;
            // Mástil
            Gizmos.DrawLine(pos, pos + Vector3.up * 1f);
            // Bandera (triángulo aproximado con líneas)
            Gizmos.DrawLine(pos + Vector3.up * 1f,   pos + new Vector3(0.5f, 0.75f, 0f));
            Gizmos.DrawLine(pos + new Vector3(0.5f, 0.75f, 0f), pos + Vector3.up * 0.5f);
            Gizmos.DrawLine(pos + Vector3.up * 0.5f, pos + Vector3.up * 1f);
            // Base
            Gizmos.DrawLine(pos + Vector3.left * 0.2f, pos + Vector3.right * 0.2f);
        }

        private static void DrawDiamond(Vector3 pos, float size, Color color)
        {
            Gizmos.color = color;
            Vector3 top   = pos + Vector3.up    * size;
            Vector3 bot   = pos + Vector3.down  * size;
            Vector3 left  = pos + Vector3.left  * size;
            Vector3 right = pos + Vector3.right * size;
            Gizmos.DrawLine(top, right);
            Gizmos.DrawLine(right, bot);
            Gizmos.DrawLine(bot, left);
            Gizmos.DrawLine(left, top);
        }

        private static void DrawPatrolArrows(Vector3 pos, float range, Color color)
        {
            Gizmos.color = color;
            float half = range * 0.5f;
            // Línea de patrullaje
            Gizmos.DrawLine(pos + Vector3.left * half, pos + Vector3.right * half);
            // Flechas en extremos
            Vector3 arrowSize = new Vector3(0.2f, 0.2f, 0f);
            Gizmos.DrawLine(pos + Vector3.right * half,
                            pos + Vector3.right * half + new Vector3(-0.25f,  0.2f, 0f));
            Gizmos.DrawLine(pos + Vector3.right * half,
                            pos + Vector3.right * half + new Vector3(-0.25f, -0.2f, 0f));
            Gizmos.DrawLine(pos + Vector3.left  * half,
                            pos + Vector3.left  * half + new Vector3( 0.25f,  0.2f, 0f));
            Gizmos.DrawLine(pos + Vector3.left  * half,
                            pos + Vector3.left  * half + new Vector3( 0.25f, -0.2f, 0f));
        }

        private static void DrawStar(Vector3 pos, float size, Color color)
        {
            Gizmos.color = color;
            int points = 5;
            float outerR = size;
            float innerR = size * 0.45f;
            Vector3 prev = Vector3.zero;
            for (int i = 0; i <= points * 2; i++)
            {
                float angle = Mathf.PI * 2f * i / (points * 2f) - Mathf.PI * 0.5f;
                float r = (i % 2 == 0) ? outerR : innerR;
                Vector3 p = pos + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
                if (i > 0) Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        private static void DrawLabel(Vector3 pos, string text, Color color)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            style.fontSize = 11;
            style.fontStyle = FontStyle.Bold;
            Handles.Label(pos, text, style);
        }
#endif
    }
}
