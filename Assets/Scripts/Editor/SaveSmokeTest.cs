// Assets/Scripts/Editor/SaveSmokeTest.cs — smoke test del guardado (S06 T5).
// Se ejecuta headless: Unity -batchmode -executeMethod Editor.SaveSmokeTest.Run
// ⚠ BORRA el slot de guardado local (empieza y termina con Delete). Solo QA.
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class SaveSmokeTest
    {
        public static void Run()
        {
            try
            {
                SaveSystem.Delete();
                Assert(!SaveSystem.HasSave, "Delete no borró el slot");
                Assert(SaveSystem.Load() == null, "Load sin archivo debe devolver null");

                var d = new SaveData { sceneName = SceneLoader.Level02Scene, si = 47.5f, cells = 2 };
                d.scannedIds.Add("SC-01");
                d.scannedIds.Add("SC-B1");
                SaveSystem.Save(d);
                Assert(SaveSystem.HasSave, "Save no escribió el slot");

                var r = SaveSystem.Load();
                Assert(r != null, "Load devolvió null tras Save");
                Assert(r.sceneName == SceneLoader.Level02Scene, "sceneName no persiste");
                Assert(Mathf.Approximately(r.si, 47.5f), "si no persiste");
                Assert(r.cells == 2, "cells no persiste");
                Assert(r.scannedIds.Count == 2 && r.scannedIds[0] == "SC-01", "scannedIds no persiste");
                Assert(!string.IsNullOrEmpty(r.savedAtUtc), "savedAtUtc vacío");

                // Patrón de AutoSave: partir del slot y mutar solo lo propio
                // debe preservar los campos de otros sistemas.
                r.difficulty = 2;
                r.musicVolume = 0.5f;
                SaveSystem.Save(r);
                var r2 = SaveSystem.Load();
                Assert(r2.difficulty == 2 && Mathf.Approximately(r2.musicVolume, 0.5f),
                    "los campos reservados (dificultad/opciones) no sobreviven al re-guardado");
                Assert(r2.cells == 2, "cells se perdió al re-guardar");

                SaveSystem.Delete();
                Assert(!SaveSystem.HasSave, "Delete final no borró");

                Debug.Log("[SaveSmokeTest] OK — round-trip completo en " + SaveSystem.FilePath);
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveSmokeTest] FALLO: " + e.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new Exception(msg);
        }
    }
}
#endif
