// World Interactions — effect range preview gizmos | by Loveseal

using UnityEditor;
using UnityEngine;

namespace Loveseal.WorldInteractions.Editor
{
    /// <summary>
    /// Draws a wire sphere per synced interaction while the config is selected, previewing its
    /// effect range. Centered on the config object for scale reference; at runtime the range
    /// follows the broadcasting player.
    /// </summary>
    internal static class WorldInteractionsGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        private static void DrawRanges(WorldInteractionsConfig config, GizmoType gizmoType)
        {
            Vector3 origin = config.transform.position;
            int index = 0;

            if (config.EnableSlow && config.SlowSynced)
                DrawRange(origin, config.SlowRange, "Slow", new Color(0.35f, 0.65f, 1f), index++);
            if (config.EnableRumble && config.RumbleSynced)
                DrawRange(origin, config.RumbleRange, "Rumble", new Color(1f, 0.6f, 0.2f), index++);

            if (config.CustomInteractions == null) return;
            foreach (var interaction in config.CustomInteractions)
            {
                if (interaction == null || !interaction.Synced) continue;
                string label = string.IsNullOrWhiteSpace(interaction.Name)
                    ? interaction.CollisionTag
                    : interaction.Name;
                var color = Color.HSVToRGB((0.35f + index * 0.13f) % 1f, 0.7f, 1f);
                DrawRange(origin, interaction.EffectRange, label, color, index++);
            }
        }

        private static void DrawRange(Vector3 origin, float range, string label, Color color, int index)
        {
            if (range <= 0f) return; // 0 = entire instance, nothing to preview

            Gizmos.color = color;
            Gizmos.DrawWireSphere(origin, range);

            var style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } };
            Handles.Label(origin + Vector3.up * (range + 0.3f + index * 0.5f),
                          $"{label} — {range:0.#} m", style);
        }
    }
}
