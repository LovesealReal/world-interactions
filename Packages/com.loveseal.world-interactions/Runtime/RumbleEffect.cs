// World Interactions — built-in Rumble effect | by Loveseal

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Pulses both controllers' haptics on an interval while active (no-op on desktop).
    /// Driven by a ContactInteraction relay.
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
