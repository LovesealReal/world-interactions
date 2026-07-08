// World Interactions — custom inspector
// by Loveseal | v1.0.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Loveseal.WorldInteractions.Editor
{
    [CustomEditor(typeof(WorldInteractionsConfig))]
    public class WorldInteractionsConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField($"World Interactions v{WorldInteractionsConfig.Version}",
                                       EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "All contact receivers, relays and effect handlers are generated when the world is " +
                "built (and when entering Play Mode) — this component just configures them, and is " +
                "stripped from the built scene. The prefab can sit anywhere; generated contacts are " +
                "pinned to the world origin, matching compatible avatar senders (see PROTOCOL.md).",
                MessageType.Info);

            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();

            DrawValidation((WorldInteractionsConfig)target);
        }

        private static void DrawValidation(WorldInteractionsConfig config)
        {
            var warnings = new List<string>();

            bool hasPresenceTag = false;
            foreach (var tag in config.PresenceTags)
                if (!string.IsNullOrWhiteSpace(tag)) hasPresenceTag = true;
            if (!hasPresenceTag)
                warnings.Add("No presence tags set — presence exemption will never apply.");

            var seenTags = new HashSet<string>();
            if (config.EnableSlow && !string.IsNullOrWhiteSpace(config.SlowTag)) seenTags.Add(config.SlowTag.Trim());
            if (config.EnableRumble && !string.IsNullOrWhiteSpace(config.RumbleTag)) seenTags.Add(config.RumbleTag.Trim());

            foreach (var interaction in config.CustomInteractions)
            {
                if (interaction == null) continue;
                string label = string.IsNullOrWhiteSpace(interaction.Name) ? "(unnamed)" : interaction.Name;

                if (string.IsNullOrWhiteSpace(interaction.CollisionTag))
                    warnings.Add($"Custom interaction '{label}' has no collision tag and will be skipped.");
                else if (!seenTags.Add(interaction.CollisionTag.Trim()))
                    warnings.Add($"Custom interaction '{label}' reuses collision tag '{interaction.CollisionTag}'.");

                if (interaction.Target == null)
                    warnings.Add($"Custom interaction '{label}' has no target behaviour assigned.");
                else if (string.IsNullOrWhiteSpace(interaction.StartEvent) &&
                         string.IsNullOrWhiteSpace(interaction.EndEvent))
                    warnings.Add($"Custom interaction '{label}' has no start or end event set.");
            }

            foreach (var warning in warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }
    }
}
