// World Interactions — custom inspector | by Loveseal

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
                "Contacts are generated at build time (and on entering Play Mode); this component " +
                "only configures them and is stripped from the built scene. Effect ranges are " +
                "previewed as wire-sphere gizmos around this object while it is selected — at " +
                "runtime each range follows the broadcasting player.",
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
            bool slowHasTag = CollectTags(config.SlowTags, seenTags);
            bool rumbleHasTag = CollectTags(config.RumbleTags, seenTags);
            if (config.EnableSlow && !slowHasTag)
                warnings.Add("Slow is enabled but has no collision tags and will be skipped.");
            if (config.EnableRumble && !rumbleHasTag)
                warnings.Add("Rumble is enabled but has no collision tags and will be skipped.");

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

        private static bool CollectTags(List<string> tags, HashSet<string> into)
        {
            bool any = false;
            if (tags == null) return false;
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                any = true;
                into.Add(tag.Trim());
            }
            return any;
        }
    }
}
