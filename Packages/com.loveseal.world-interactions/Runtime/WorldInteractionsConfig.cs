// World Interactions — by Loveseal | v1.0.0

using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>A contact tag the world listens for, and the creator code to run on start/end.</summary>
    [Serializable]
    public class WorldInteraction
    {
        [Tooltip("Display name; also used for the generated GameObject.")]
        public string Name = "New Interaction";

        [Tooltip("Collision tag the avatar broadcasts (e.g. 'MyWorld/Confetti').")]
        public string CollisionTag = "";

        [Tooltip("Off: events fire only on the broadcaster's own client. On: state is synced and " +
                 "other clients react too (late joiners included).")]
        public bool Synced = false;

        [Tooltip("Players: events fire per player as they enter/leave the range. World Objects: " +
                 "events fire while broadcasting, and your handler queries the range per object " +
                 "via GetIntensity (see ProximityObjectToggler).")]
        public RangeMode Mode = RangeMode.Players;

        [Tooltip("Effect radius in meters around the broadcasting player. 0 = unlimited.")]
        [Range(0f, 100f)]
        public float EffectRange = 8f;

        [Tooltip("How intensity fades from the broadcaster (1) to the range edge (0). Read it " +
                 "via the relay's LocalIntensity/GetIntensity.")]
        public FalloffMode Falloff = FalloffMode.Linear;

        [Tooltip("Skip the events on clients whose local player broadcasts a Presence Tag.")]
        public bool ExemptPresence = false;

        [Tooltip("Your UdonSharp behaviour that reacts to this interaction.")]
        public UdonSharpBehaviour Target;

        [Tooltip("Custom event sent to Target when the effect starts.")]
        public string StartEvent = "OnInteractionStart";

        [Tooltip("Custom event sent to Target when the effect ends. Leave empty for none.")]
        public string EndEvent = "OnInteractionEnd";
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("World Interactions/World Interactions")]
    public class WorldInteractionsConfig : MonoBehaviour, IEditorOnly
    {
        public const string Version = "1.0.0";

        [Header("Contacts")]
        [Tooltip("Radius of the generated contact receivers. Compatible senders are 0.5m spheres " +
                 "at the world origin, so any positive value overlaps them.")]
        [Range(0.1f, 5f)]
        public float ReceiverRadius = 0.5f;

        [Header("Presence")]
        [Tooltip("Tags that mark the local player as a presence-beacon wearer (e.g. a staff badge).")]
        public List<string> PresenceTags = new List<string>
        {
            "WI/Presence",
        };

        [Header("Slow")]
        [Tooltip("React to Slow broadcasts by reducing player movement speed.")]
        public bool EnableSlow = true;

        [Tooltip("Collision tags Slow listens for. Add vendor tags (e.g. 'MyClub/Slow') to " +
                 "support other ecosystems.")]
        public List<string> SlowTags = new List<string>
        {
            "WI/Slow",
        };

        [Tooltip("Movement speed is multiplied by this while Slow is active.")]
        [Range(0f, 1f)]
        public float SlowSpeedMultiplier = 0.5f;

        [Tooltip("Sync Slow so players near the broadcaster are affected (presence wearers exempt). " +
                 "Off = only the broadcaster's own client.")]
        public bool SlowSynced = true;

        [Tooltip("Slow radius in meters around the broadcaster. 0 = entire instance. " +
                 "Previewed as a gizmo while this object is selected.")]
        [Range(0f, 100f)]
        public float SlowRange = 8f;

        [Tooltip("How the slow fades with distance: full multiplier at the broadcaster, normal " +
                 "speed at the range edge.")]
        public FalloffMode SlowFalloff = FalloffMode.Linear;

        [Header("Rumble")]
        [Tooltip("React to Rumble broadcasts by pulsing controller haptics.")]
        public bool EnableRumble = true;

        [Tooltip("Collision tags Rumble listens for. Add vendor tags (e.g. 'MyClub/Rumble') to " +
                 "support other ecosystems.")]
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

        [Tooltip("Sync Rumble so players near the broadcaster are affected (presence wearers exempt). " +
                 "Off = only the broadcaster's own client.")]
        public bool RumbleSynced = true;

        [Tooltip("Rumble radius in meters around the broadcaster. 0 = entire instance. " +
                 "Previewed as a gizmo while this object is selected.")]
        [Range(0f, 100f)]
        public float RumbleRange = 8f;

        [Tooltip("How the rumble strength fades with distance from the broadcaster.")]
        public FalloffMode RumbleFalloff = FalloffMode.Linear;

        [Header("Custom Interactions")]
        [Tooltip("Your own contact tags and the code to run when they trigger.")]
        public List<WorldInteraction> CustomInteractions = new List<WorldInteraction>();
    }
}
