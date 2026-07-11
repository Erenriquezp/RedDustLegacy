using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Guardado en disco — S06 T5. Un solo slot, JSON sin encriptar (prototipo) en
/// <c>Application.persistentDataPath/save.json</c>. Estático y sin estado: quien
/// guarda decide cuándo (GameManager autosave al entrar a nivel y al registrar
/// checkpoint). La victoria (S05 T4, CinematicManager) debe llamar a
/// <see cref="Delete"/> para que la partida cierre limpia.
/// </summary>
public static class SaveSystem
{
    private const string FileName = "save.json";

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave => File.Exists(FilePath);

    /// <summary>Escribe el slot. Nunca lanza: un disco lleno no debe tirar el juego.</summary>
    public static void Save(SaveData data)
    {
        if (data == null) return;
        try
        {
            data.savedAtUtc = DateTime.UtcNow.ToString("o");
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, prettyPrint: true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] No se pudo guardar en {FilePath}: {e.Message}");
        }
    }

    /// <summary>Lee el slot. Devuelve null si no existe o está corrupto.</summary>
    public static SaveData Load()
    {
        try
        {
            if (!HasSave) return null;
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Guardado ilegible ({e.Message}); se ignora {FilePath}.");
            return null;
        }
    }

    public static void Delete()
    {
        try
        {
            if (HasSave) File.Delete(FilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] No se pudo borrar {FilePath}: {e.Message}");
        }
    }
}
