// World Interactions — build-time generator
// by Loveseal | v1.0.0
//
// Nothing but a config component lives in the scene; every contact receiver, relay and
// effect handler is generated when the world builds (and when entering Play Mode), then
// the config is stripped from the built scene.

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

        // Must run before UdonSharp's [PostProcessScene] pass (order 0), which copies proxy
        // state onto the backing UdonBehaviours and strips proxies from player builds, and
        // before VRC's UdonBuildPreprocessor (order 0), which populates the serialized
        // program asset references — both need to see the behaviours generated here.
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

            // Drop any stale generated root so re-processing never duplicates objects.
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == GeneratedRootName)
                    Object.DestroyImmediate(root);

            Generate(scene, configs[0]);

            // The config is editor-only; keep it out of the built scene. If it sits on a prefab
            // instance, unpack first — Unity refuses to strip components off prefab instances.
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
                    // Harmless if it stays: it's an IEditorOnly MonoBehaviour, which the client ignores.
                    Debug.LogWarning($"[WorldInteractions] Could not strip config component: {ex.Message}");
                }
            }
        }

        private static void Generate(Scene scene, WorldInteractionsConfig config)
        {
            var generatedRoot = new GameObject(GeneratedRootName);
            SceneManager.MoveGameObjectToScene(generatedRoot, scene);
            // Receivers must coincide with the avatar senders pinned to the world origin.
            generatedRoot.transform.position = Vector3.zero;
            generatedRoot.transform.rotation = Quaternion.identity;

            int created = 0;

            // Presence detector — always built, so effects can exempt beacon wearers and creator
            // code can check PresenceDetector.IsLocalPlayerBroadcasting.
            var detectorGo = CreateChild(generatedRoot, "Presence Detector");
            var detector = detectorGo.AddUdonSharpComponent<PresenceDetector>();
            AddReceiver(detectorGo, config.PresenceTags, config.ReceiverRadius);
            UdonSharpEditorUtility.CopyProxyToUdon(detector);

            // Built-in Slow.
            if (config.EnableSlow && HasAnyTag(config.SlowTags))
            {
                var go = CreateChild(generatedRoot, "Slow");
                var effect = go.AddUdonSharpComponent<SlowEffect>();
                effect.SpeedMultiplier = config.SlowSpeedMultiplier;
                UdonSharpEditorUtility.CopyProxyToUdon(effect);

                AddRelay(go, "Slow", config.SlowTags, effect,
                         nameof(SlowEffect.OnSlowStart), nameof(SlowEffect.OnSlowEnd),
                         config.SlowInstanceWide, exemptPresence: true, detector, config.ReceiverRadius);
                created++;
            }

            // Built-in Rumble.
            if (config.EnableRumble && HasAnyTag(config.RumbleTags))
            {
                var go = CreateChild(generatedRoot, "Rumble");
                var effect = go.AddUdonSharpComponent<RumbleEffect>();
                effect.PulseInterval = config.RumblePulseInterval;
                effect.Amplitude     = config.RumbleAmplitude;
                effect.Frequency     = config.RumbleFrequency;
                UdonSharpEditorUtility.CopyProxyToUdon(effect);

                AddRelay(go, "Rumble", config.RumbleTags, effect,
                         nameof(RumbleEffect.OnRumbleStart), nameof(RumbleEffect.OnRumbleEnd),
                         config.RumbleInstanceWide, exemptPresence: true, detector, config.ReceiverRadius);
                created++;
            }

            // Creator-defined interactions.
            var usedNames = new HashSet<string> { "Slow", "Rumble", "Presence Detector" };
            foreach (var interaction in config.CustomInteractions)
            {
                if (interaction == null) continue;
                if (string.IsNullOrWhiteSpace(interaction.CollisionTag))
                {
                    Debug.LogWarning($"[WorldInteractions] Custom interaction '{interaction.Name}' has no " +
                                     "collision tag. Skipping.");
                    continue;
                }
                if (interaction.Target == null)
                {
                    Debug.LogWarning($"[WorldInteractions] Custom interaction '{interaction.Name}' has no " +
                                     "target behaviour; it will track state but fire no events.");
                }

                string name = string.IsNullOrWhiteSpace(interaction.Name) ? interaction.CollisionTag : interaction.Name.Trim();
                if (!usedNames.Add(name))
                {
                    Debug.LogWarning($"[WorldInteractions] Duplicate interaction name '{name}'. Skipping.");
                    continue;
                }

                var go = CreateChild(generatedRoot, name);
                AddRelay(go, name, new[] { interaction.CollisionTag }, interaction.Target,
                         interaction.StartEvent, interaction.EndEvent,
                         interaction.InstanceWide, interaction.ExemptPresence, detector, config.ReceiverRadius);
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

        private static void AddRelay(GameObject go, string name, IEnumerable<string> tags,
                                     UdonSharp.UdonSharpBehaviour target,
                                     string startEvent, string endEvent, bool instanceWide, bool exemptPresence,
                                     PresenceDetector detector, float radius)
        {
            var relay = go.AddUdonSharpComponent<ContactInteraction>();
            relay.InteractionName = name;
            relay.Target          = target;
            relay.StartEvent      = startEvent;
            relay.EndEvent        = endEvent;
            relay.InstanceWide    = instanceWide;
            relay.ExemptPresence  = exemptPresence;
            relay.Detector        = detector;
            UdonSharpEditorUtility.CopyProxyToUdon(relay);

            var receiver = AddReceiver(go, tags, radius);
            Debug.Log($"[WorldInteractions] Created interaction '{name}' " +
                      $"(tags '{string.Join("', '", receiver.collisionTags)}', " +
                      $"{(instanceWide ? "instance-wide" : "local")}{(exemptPresence ? ", presence exempt" : "")}).");
        }

        // The receiver must sit on the same GameObject as the relay: in worlds, a receiver
        // sends _onContactEnter/_onContactExit Udon events to the behaviours on its own object.
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
