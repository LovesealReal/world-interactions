// World Interactions — menu items for spawning the prefab.
// Because this ships as a VPM package (not under Assets/), users can't drag the
// prefab from the Project window, so these menu entries do it for them.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Loveseal.WorldInteractions.Editor
{
    public static class WorldInteractionsMenu
    {
        // GUID of "World Interactions.prefab" at the package root.
        private const string PrefabGuid = "66eda577d71041fc8cc90e28a2a19bf2";

        [MenuItem("Tools/World Interactions/Initialize World Interactions", false, 0)]
        private static void InitializeFromToolsMenu()
        {
            SpawnPrefab(null, PrefabGuid, "World Interactions");
        }

        // Adding the "GameObject/" prefix makes this show up in the hierarchy
        // right-click context menu (and the top GameObject menu).
        [MenuItem("GameObject/World Interactions", false, 10)]
        private static void InitializeFromHierarchy(MenuCommand command)
        {
            SpawnPrefab(command.context as GameObject, PrefabGuid, "World Interactions");
        }

        /// <summary>Shared prefab spawner, reusable by vendor preset packages.</summary>
        public static void SpawnPrefab(GameObject parent, string prefabGuid, string displayName)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                EditorUtility.DisplayDialog(
                    displayName,
                    $"Could not find the {displayName} prefab. Make sure the " +
                    "package is installed correctly.",
                    "OK");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null)
            {
                return;
            }

            if (parent != null)
            {
                Undo.SetTransformParent(instance.transform, parent.transform, $"Spawn {displayName}");
            }
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;

            // Keep it in the scene the user is looking at.
            var targetScene = parent != null
                ? parent.scene
                : SceneManager.GetActiveScene();
            if (targetScene.IsValid() && instance.scene != targetScene)
            {
                SceneManager.MoveGameObjectToScene(instance, targetScene);
            }

            Undo.RegisterCreatedObjectUndo(instance, $"Spawn {displayName}");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
            EditorSceneManager.MarkSceneDirty(instance.scene);
        }
    }
}
