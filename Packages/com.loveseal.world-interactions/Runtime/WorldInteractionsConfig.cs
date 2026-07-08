// World Interactions — by Loveseal | v1.0.0
// World-side receiver system for avatar contact broadcasts. See PROTOCOL.md for the
// contact convention avatars follow to be compatible.

using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// One custom interaction: a collision tag the world listens for, and the creator's
    /// UdonSharp behaviour + events to run when a matching avatar contact starts/ends.
    /// </summary>
    [Serializable]
    public class WorldInteraction
    {
        [Tooltip("Display name; also used for the generated GameObject.")]
        public string Name = "New Interaction";

        [Tooltip("Collision tag the avatar's contact sender broadcasts (e.g. 'MyWorld/Confetti').")]
        public string CollisionTag = "";

        [Tooltip("Off: events fire only on the client that detects the contact (the player whose " +
                 "avatar sent it). On: the state is synced so the events fire for everyone in the " +
                 "instance, including late joiners.")]
        public bool InstanceWide = false;

        [Tooltip("Skip firing the events on clients whose local player is broadcasting one of the " +
                 "Presence Tags (e.g. staff badges).")]
        public bool ExemptPresence = false;

        [Tooltip("Your UdonSharp behaviour that reacts to this interaction.")]
        public UdonSharpBehaviour Target;

        [Tooltip("Custom event sent to Target when the contact starts.")]
        public string StartEvent = "OnInteractionStart";

        [Tooltip("Custom event sent to Target when the contact ends. Leave empty for none.")]
        public string EndEvent = "OnInteractionEnd";
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("World Interactions/World Interactions")]
    public class WorldInteractionsConfig : MonoBehaviour, IEditorOnly
    {
        public const string Version = "1.0.0";

        [Header("Contacts")]
        [Tooltip("Radius of the generated contact receivers. Compatible avatar senders are 0.5m " +
                 "spheres pinned to the world origin, so any positive value overlaps them.")]
        [Range(0.1f, 5f)]
        public float ReceiverRadius = 0.5f;

        [Header("Presence")]
        [Tooltip("Tags that mark the local player as a presence-beacon wearer (e.g. a staff badge). " +
                 "Players broadcasting one of these can be exempted from effects, and creator code " +
                 "can check PresenceDetector.IsLocalPlayerBroadcasting.")]
        public List<string> PresenceTags = new List<string>
        {
            "WI/Presence",
        };

        [Header("Slow")]
        [Tooltip("React to Slow broadcasts by reducing player movement speed.")]
        public bool EnableSlow = true;

        [Tooltip("Collision tags the Slow feature listens for. Add vendor tags (e.g. " +
                 "'MyClub/Slow') alongside the standard one to support other ecosystems.")]
        public List<string> SlowTags = new List<string>
        {
            "WI/Slow",
        };

        [Tooltip("Movement speed is multiplied by this while Slow is active.")]
        [Range(0f, 1f)]
        public float SlowSpeedMultiplier = 0.5f;

        [Tooltip("Apply Slow to everyone in the instance (presence wearers exempt). Off = only " +
                 "the player whose avatar sent the contact.")]
        public bool SlowInstanceWide = true;

        [Header("Rumble")]
        [Tooltip("React to Rumble broadcasts by pulsing controller haptics.")]
        public bool EnableRumble = true;

        [Tooltip("Collision tags the Rumble feature listens for. Add vendor tags (e.g. " +
                 "'MyClub/Rumble') alongside the standard one to support other ecosystems.")]
        public List<string> RumbleTags = new List<string>
        {
            "WI/Rumble",
        };

        [Tooltip("Seconds between haptic pulses (also each pulse's duration).")]
        [Range(0.05f, 2f)]
        public float RumblePulseInterval = 0.25f;

        [Tooltip("Haptic pulse strength (0-1).")]
        [Range(0f, 1f)]
        public float RumbleAmplitude = 0.7f;

        [Tooltip("Haptic pulse frequency (0-1).")]
        [Range(0f, 1f)]
        public float RumbleFrequency = 0.5f;

        [Tooltip("Apply Rumble to everyone in the instance (presence wearers exempt). Off = only " +
                 "the player whose avatar sent the contact.")]
        public bool RumbleInstanceWide = true;

        [Header("Custom Interactions")]
        [Tooltip("Your own contact tags and the code to run when they trigger.")]
        public List<WorldInteraction> CustomInteractions = new List<WorldInteraction>();
    }
}
