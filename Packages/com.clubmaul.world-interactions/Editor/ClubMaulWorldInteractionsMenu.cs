// Club Maul World Interactions — menu items for spawning the Club Maul preset prefab.

using Loveseal.WorldInteractions.Editor;
using UnityEditor;
using UnityEngine;

namespace ClubMaul.WorldInteractions.Editor
{
    public static class ClubMaulWorldInteractionsMenu
    {
        // GUID of "Club Maul World Interactions.prefab" at the package root.
        private const string PrefabGuid = "4efa1f6e24a04416936be43fc383b47d";

        [MenuItem("Tools/Club Maul/Initialize World Interactions", false, 0)]
        private static void InitializeFromToolsMenu()
        {
            WorldInteractionsMenu.SpawnPrefab(null, PrefabGuid, "Club Maul World Interactions");
        }

        [MenuItem("GameObject/Club Maul/World Interactions", false, 10)]
        private static void InitializeFromHierarchy(MenuCommand command)
        {
            WorldInteractionsMenu.SpawnPrefab(command.context as GameObject, PrefabGuid,
                                              "Club Maul World Interactions");
        }
    }
}
