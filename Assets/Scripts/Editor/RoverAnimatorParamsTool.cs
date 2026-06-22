// Assets/Scripts/Editor/RoverAnimatorParamsTool.cs
// SOLO para el Editor — NO se incluye en builds.
//
// Añade al RoverAC los parámetros del Animator que escribe
// PlayerAnimatorController pero que pueden faltar en el controller.
// No duplica parámetros existentes (Speed, IsGrounded, etc.).
//
// USO:
//   Unity Menu → Tools → Red Dust → Add Rover Animator Params

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
    public static class RoverAnimatorParamsTool
    {
        private const string CONTROLLER_PATH = "Assets/Animations/Rover/RoverAC.controller";

        // Parámetros objetivo y su tipo.
        private static readonly (string name, AnimatorControllerParameterType type)[] _wanted =
        {
            ("VelocityY", AnimatorControllerParameterType.Float),
            ("IsDashing", AnimatorControllerParameterType.Bool),
            ("IsOnWall",  AnimatorControllerParameterType.Bool),
            ("IsDead",    AnimatorControllerParameterType.Bool),
            ("IsDamaged", AnimatorControllerParameterType.Trigger),
        };

        [MenuItem("Tools/Red Dust/Add Rover Animator Params")]
        public static void AddParams()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "RoverAC no encontrado",
                    $"No se encontró el Animator Controller en:\n{CONTROLLER_PATH}",
                    "OK");
                return;
            }

            int added = 0, skipped = 0;

            foreach (var (name, type) in _wanted)
            {
                if (HasParameter(controller, name))
                {
                    skipped++;
                    continue;
                }

                controller.AddParameter(name, type);
                added++;
                Debug.Log($"[RoverAnimatorParamsTool] Añadido: {name} ({type})");
            }

            if (added > 0)
            {
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[RoverAnimatorParamsTool] Listo. Añadidos: {added}, ya existían: {skipped}.");
            EditorGUIUtility.PingObject(controller);
        }

        private static bool HasParameter(AnimatorController controller, string name)
        {
            foreach (var p in controller.parameters)
                if (p.name == name)
                    return true;
            return false;
        }
    }
}
#endif
