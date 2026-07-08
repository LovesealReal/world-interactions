// World Interactions — built-in Rumble effect
// by Loveseal | v1.0.0

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Built-in handler for the standard Rumble broadcast. While active, both of
    /// the local player's controllers pulse on an interval (no-op for desktop players).
    /// Driven by a ContactInteraction relay via OnRumbleStart/OnRumbleEnd.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RumbleEffect : UdonSharpBehaviour
    {
        [Tooltip("Seconds between haptic pulses (also each pulse's duration).")]
        [Range(0.05f, 2f)]
        public float PulseInterval = 0.25f;

        [Tooltip("Haptic pulse strength (0-1).")]
        [Range(0f, 1f)]
        public float Amplitude = 0.7f;

        [Tooltip("Haptic pulse frequency (0-1).")]
        [Range(0f, 1f)]
        public float Frequency = 0.5f;

        private bool _active;
        private bool _pulseScheduled;

        public void OnRumbleStart()
        {
            _active = true;
            // A previously scheduled pulse keeps the loop alive; only start a new one if none is pending.
            if (!_pulseScheduled) _RumblePulse();
        }

        public void OnRumbleEnd()
        {
            _active = false;
        }

        public void _RumblePulse()
        {
            if (!_active)
            {
                _pulseScheduled = false;
                return;
            }

            VRCPlayerApi player = Networking.LocalPlayer;
            if (player != null && player.IsUserInVR())
            {
                player.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, PulseInterval, Amplitude, Frequency);
                player.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, PulseInterval, Amplitude, Frequency);
            }

            _pulseScheduled = true;
            SendCustomEventDelayedSeconds(nameof(_RumblePulse), PulseInterval);
        }
    }
}
