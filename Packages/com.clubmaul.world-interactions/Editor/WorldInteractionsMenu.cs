// World Interactions — menu items for spawning the prefab.
// Because this ships as a VPM package (not under Assets/), users can't drag the
// prefab from the Project window, so these menu entries do it for them.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClubMaul.WorldInteractions.Editor
{
    public static class WorldInteractionsMenu
    {
        // GUID of "Club Maul World Interactions.prefab" at the package root.
        private const string PrefabGuid = "66eda577d71041fc8cc90e28a2a19bf2";

        [MenuItem("Tools/Club Maul/Initialize World Interactions", false, 0)]
        private static void InitializeFromToolsMenu()
        {
            SpawnWorldInteractions(null);
        }

        // Adding the "GameObject/" prefix makes this show up in the hierarchy
        // right-click context menu (and the top GameObject menu).
        [MenuItem("GameObject/Club Maul/World Interactions", false, 10)]
        private static void InitializeFromHierarchy(MenuCommand command)
        {
            SpawnWorldInteractions(command.context as GameObject);
        }

        private static void SpawnWorldInteractions(GameObject parent)
        {
            var path = AssetDatabase.GUIDToAssetPath(PrefabGuid);
            var prefab = string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                EditorUtility.DisplayDialog(
                    "World Interactions",
                    "Could not find the World Interactions prefab. Make sure the " +
                    "Club Maul World Interactions package is installed correctly.",
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
                Undo.SetTransformParent(instance.transform, parent.transform, "Spawn World Interactions");
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

            Undo.RegisterCreatedObjectUndo(instance, "Spawn World Interactions");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
            EditorSceneManager.MarkSceneDirty(instance.scene);
        }
    }
}
