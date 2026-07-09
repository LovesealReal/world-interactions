// World Interactions — build-time generator | by Loveseal
//
// The scene only holds a config component; receivers, relays and effect handlers are
// generated at build time (and on entering Play Mode), then the config is stripped.

using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

namespace Loveseal.WorldInteractions.Editor
{
    public class WorldInteractionsBuilder : IProcessSceneWithReport
    {
        public const string GeneratedRootName = "World Interactions (Generated)";

        // Before UdonSharp's [PostProcessScene] pass and VRC's UdonBuildPreprocessor (both
        // order 0) — they must see the behaviours generated here.
        public int callbackOrder => -1024;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var configs = new List<WorldInteractionsConfig>();
            foreach (var root in scene.GetRootGameObjects())
                configs.AddRange(root.GetComponentsInChildren<WorldInteractionsConfig>(true));

            if (configs.Count == 0) return;
            if (configs.Count > 1)
            {
                Debug.LogWarning($"[WorldInteractions] {configs.Count} World Interactions components " +
                                 "found; only the first is used. Remove the extras.");
            }

            foreach (var root in scene.GetRootGameObjects())
                if (root.name == GeneratedRootName)
                    Object.DestroyImmediate(root);

            Generate(scene, configs[0]);

            // Strip the editor-only config from the built scene. Prefab instances must be
            // unpacked first — Unity refuses to remove their components.
            foreach (var config in configs)
            {
                try
                {
                    if (PrefabUtility.IsPartOfPrefabInstance(config))
                    {
                        var instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(config.gameObject);
                        if (instanceRoot != null)
                            PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely,
                                                               InteractionMode.AutomatedAction);
                    }
                    Object.DestroyImmediate(config);
                }
                catch (System.Exception ex)
                {
                    // Harmless if it stays: the client ignores IEditorOnly MonoBehaviours.
                    Debug.LogWarning($"[WorldInteractions] Could not strip config component: {ex.Message}");
                }
            }
        }

        private static void Generate(Scene scene, WorldInteractionsConfig config)
        {
            // Receivers must coincide with the senders pinned to the world origin.
            var generatedRoot = new GameObject(GeneratedRootName);
            SceneManager.MoveGameObjectToScene(generatedRoot, scene);
            generatedRoot.transform.position = Vector3.zero;
            generatedRoot.transform.rotation = Quaternion.identity;

            int created = 0;

            var detectorGo = CreateChild(generatedRoot, "Presence Detector");
            var detector = detectorGo.AddUdonSharpComponent<PresenceDetector>();
            AddReceiver(detectorGo, config.PresenceTags, config.ReceiverRadius);
            UdonSharpEditorUtility.CopyProxyToUdon(detector);

            if (config.EnableSlow && HasAnyTag(config.SlowTags))
            {
                var go = CreateChild(generatedRoot, "Slow");
                var effect = go.AddUdonSharpComponent<SlowEffect>();
                effect.SpeedMultiplier = config.SlowSpeedMultiplier;

                effect.SourceInteraction = AddRelay(go, "Slow", config.SlowTags, effect,
                    nameof(SlowEffect.OnSlowStart), nameof(SlowEffect.OnSlowEnd),
                    config.SlowSynced, config.SlowRange, config.SlowFalloff, RangeMode.Players,
                    exemptPresence: true, detector, config.ReceiverRadius);
                UdonSharpEditorUtility.CopyProxyToUdon(effect);
                created++;
            }

            if (config.EnableRumble && HasAnyTag(config.RumbleTags))
            {
                var go = CreateChild(generatedRoot, "Rumble");
                var effect = go.AddUdonSharpComponent<RumbleEffect>();
                effect.PulseInterval = config.RumblePulseInterval;
                effect.Amplitude     = config.RumbleAmplitude;
                effect.Frequency     = config.RumbleFrequency;

                effect.SourceInteraction = AddRelay(go, "Rumble", config.RumbleTags, effect,
                    nameof(RumbleEffect.OnRumbleStart), nameof(RumbleEffect.OnRumbleEnd),
                    config.RumbleSynced, config.RumbleRange, config.RumbleFalloff, RangeMode.Players,
                    exemptPresence: true, detector, config.ReceiverRadius);
                UdonSharpEditorUtility.CopyProxyToUdon(effect);
                created++;
            }

            var usedNames = new HashSet<string> { "Slow", "Rumble", "Presence Detector" };
            foreach (var interaction in config.CustomInteractions)
            {
                if (interaction == null) continue;
                if (!HasAnyTag(interaction.CollisionTags))
                {
                    Debug.LogWarning($"[WorldInteractions] Custom interaction '{interaction.Name}' has no " +
                                     "collision tags. Skipping.");
                    continue;
                }
                if (interaction.Target == null)
                {
                    Debug.LogWarning($"[WorldInteractions] Custom interaction '{interaction.Name}' has no " +
                                     "target behaviour; it will track state but fire no events.");
                }

                string name = string.IsNullOrWhiteSpace(interaction.Name)
                    ? FirstTag(interaction.CollisionTags)
                    : interaction.Name.Trim();
                if (!usedNames.Add(name))
                {
                    Debug.LogWarning($"[WorldInteractions] Duplicate interaction name '{name}'. Skipping.");
                    continue;
                }

                var go = CreateChild(generatedRoot, name);
                AddRelay(go, name, interaction.CollisionTags, interaction.Target,
                         interaction.StartEvent, interaction.EndEvent,
                         interaction.Synced, interaction.EffectRange, interaction.Falloff,
                         interaction.Mode, interaction.ExemptPresence, detector, config.ReceiverRadius);
                created++;
            }

            Debug.Log($"[WorldInteractions] Generated presence detector + {created} interaction contact(s) " +
                      $"at world origin for scene '{scene.name}'.");
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static bool HasAnyTag(IEnumerable<string> tags)
        {
            if (tags == null) return false;
            foreach (var tag in tags)
                if (!string.IsNullOrWhiteSpace(tag)) return true;
            return false;
        }

        private static string FirstTag(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
                if (!string.IsNullOrWhiteSpace(tag)) return tag.Trim();
            return "Interaction";
        }

        private static ContactInteraction AddRelay(GameObject go, string name, IEnumerable<string> tags,
                                                   UdonSharp.UdonSharpBehaviour target,
                                                   string startEvent, string endEvent, bool synced,
                                                   float effectRange, FalloffMode falloff, RangeMode mode,
                                                   bool exemptPresence, PresenceDetector detector, float radius)
        {
            var relay = go.AddUdonSharpComponent<ContactInteraction>();
            relay.InteractionName = name;
            relay.Target          = target;
            relay.StartEvent      = startEvent;
            relay.EndEvent        = endEvent;
            relay.Synced          = synced;
            relay.EffectRange     = effectRange;
            relay.Falloff         = falloff;
            relay.Mode            = mode;
            relay.ExemptPresence  = exemptPresence;
            relay.Detector        = detector;
            UdonSharpEditorUtility.CopyProxyToUdon(relay);

            var receiver = AddReceiver(go, tags, radius);
            string scope = !synced ? "broadcaster only"
                : effectRange > 0f ? $"synced, {effectRange:0.#}m range, {falloff} falloff" : "synced, unlimited";
            Debug.Log($"[WorldInteractions] Created interaction '{name}' " +
                      $"(tags '{string.Join("', '", receiver.collisionTags)}', {scope}, {mode}" +
                      $"{(exemptPresence ? ", presence exempt" : "")}).");
            return relay;
        }

        // The receiver must share a GameObject with the relay: in worlds, receivers emit
        // contact events only to behaviours on their own object.
        private static VRCContactReceiver AddReceiver(GameObject go, IEnumerable<string> tags, float radius)
        {
            var cleanTags = new List<string>();
            foreach (var tag in tags)
                if (!string.IsNullOrWhiteSpace(tag))
                    cleanTags.Add(tag.Trim());

            var receiver = go.AddComponent<VRCContactReceiver>();
            receiver.shapeType     = ContactBase.ShapeType.Sphere;
            receiver.radius        = radius;
            receiver.position      = Vector3.zero;
            receiver.rotation      = Quaternion.identity;
            receiver.allowSelf     = true;
            receiver.allowOthers   = true;
            receiver.localOnly     = false;
            receiver.receiverType  = ContactReceiver.ReceiverType.Constant;
            receiver.collisionTags = cleanTags;
            return receiver;
        }
    }
}
