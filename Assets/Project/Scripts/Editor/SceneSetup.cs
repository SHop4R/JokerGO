using JokerGO.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace JokerGO.Editor
{
    /// <summary>One-click scene wiring: ensures SampleScene contains the GameBootstrap object.</summary>
    public static class SceneSetup
    {
        private const string ScenePath = "Assets/Project/Scenes/SampleScene.unity";
        private const string BootstrapName = "GameBootstrap";

        [MenuItem("JokerGO/Setup Scene _F9")]
        public static void Apply()
        {
            // Gives the user a chance to save (or cancel) if another scene has unsaved edits.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath);

            if (Object.FindFirstObjectByType<GameBootstrap>() != null)
            {
                Debug.Log("[JokerGO] Scene already wired; nothing to do.");
                return;
            }

            var go = new GameObject(BootstrapName);
            go.AddComponent<GameBootstrap>();
            Undo.RegisterCreatedObjectUndo(go, "Create GameBootstrap");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[JokerGO] {BootstrapName} added to {ScenePath} and scene saved.");
        }
    }
}
