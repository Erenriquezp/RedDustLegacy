using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreatePlayerPrefab
{
    [MenuItem("Tools/Create Player Prefab")]
    public static void CreatePrefab()
    {
        PlayerController playerController = GameObject.FindAnyObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("❌ No se encontró ningún GameObject con el componente PlayerController en la escena activa.");
            EditorUtility.DisplayDialog("Error", "No se encontró ningún GameObject con el componente PlayerController en la escena activa.", "OK");
            return;
        }

        GameObject playerGameObject = playerController.gameObject;
        string folderPath = "Assets/Prefabs/Player";
        string prefabPath = folderPath + "/Player.prefab";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(playerGameObject, prefabPath, out bool success);

        if (success && savedPrefab != null)
        {
            Debug.Log($"✅ Prefab del jugador creado con éxito en: {prefabPath}");
            EditorUtility.DisplayDialog("Éxito", $"Prefab del jugador creado con éxito en: {prefabPath}", "OK");
        }
        else
        {
            Debug.LogError("❌ Falló la creación del prefab del jugador.");
            EditorUtility.DisplayDialog("Error", "Falló la creación del prefab del jugador.", "OK");
        }
    }
}
