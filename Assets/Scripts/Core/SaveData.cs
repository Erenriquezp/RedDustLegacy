using System;
using System.Collections.Generic;

/// <summary>
/// Estado persistente de la partida — S06 T5 (un solo slot). Serializado a JSON
/// por <see cref="SaveSystem"/>. Los campos que aún no tienen sistema dueño
/// (upgrades S05 T5, códex S06 T4, dificultad/opciones S06 T2) ya existen aquí
/// para que esos sprints solo tengan que leer/escribirlos: JsonUtility tolera
/// campos ausentes en guardados viejos, así que añadir campos no rompe nada.
/// </summary>
[Serializable]
public class SaveData
{
    public int version = 1;

    // ── Progreso (los escribe GameManager) ─────────────────────────────────
    public string sceneName = SceneLoader.Level01Scene;
    public float si = 100f;
    public int cells = 0;
    public List<string> scannedIds = new List<string>();   // fichas SC-XX (lore, GDD §11)

    // ── Reservado para sprints siguientes ──────────────────────────────────
    public List<string> upgrades = new List<string>();      // S05 T5 — UpgradeManager
    public List<string> codexUnlocked = new List<string>(); // S06 T4 — códex
    public int difficulty = 1;                               // S06 T2 — 0 Fácil / 1 Normal / 2 Difícil
    public float musicVolume = 1f;                           // S06 T2 — opciones de audio
    public float sfxVolume = 1f;
    public float ambientVolume = 1f;
    public float uiVolume = 1f;

    public string savedAtUtc = "";
}
