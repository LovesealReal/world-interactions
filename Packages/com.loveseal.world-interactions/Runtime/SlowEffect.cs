// World Interactions — built-in Slow effect | by Loveseal

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Scales the local player's walk/run/strafe speed while active, weighted by the
    /// interaction's falloff intensity (full slow at the broadcaster, normal speed at the
    /// range edge). Original speeds are restored on end.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SlowEffect : UdonSharpBehaviour
    {
        private const float UpdateInterval = 0.25f;

        [Tooltip("Movement speed multiplier at full intensity.")]
        [Range(0f, 1f)]
        public float SpeedMultiplier = 0.5f;

        [HideInInspector] public ContactInteraction SourceInteraction; // injected by the relay

        private bool _applied;
        private bool _loopScheduled;
        private float _walkSpeed;
        private float _runSpeed;
        private float _strafeSpeed;

        public void OnSlowStart()
        {
            if (_applied) return;
            VRCPlayerApi player = Networking.LocalPlayer;
            if (player == null) return;

            _walkSpeed   = player.GetWalkSpeed();
            _runSpeed    = player.GetRunSpeed();
            _strafeSpeed = player.GetStrafeSpeed();
            _applied = true;

            if (!_loopScheduled) _SlowUpdate();
        }

        public void OnSlowEnd()
        {
            if (!_applied) return;
            _applied = false;

            VRCPlayerApi player = Networking.LocalPlayer;
            if (player == null) return;
            player.SetWalkSpeed(_walkSpeed);
            player.SetRunSpeed(_runSpeed);
            player.SetStrafeSpeed(_strafeSpeed);
        }

        public void _SlowUpdate()
        {
            _loopScheduled = false;
            if (!_applied) return;

            VRCPlayerApi player = Networking.LocalPlayer;
            if (player != null)
            {
                float intensity = SourceInteraction != null ? SourceInteraction.LocalIntensity : 1f;
                float multiplier = Mathf.Lerp(1f, SpeedMultiplier, intensity);
                player.SetWalkSpeed(_walkSpeed * multiplier);
                player.SetRunSpeed(_runSpeed * multiplier);
                player.SetStrafeSpeed(_strafeSpeed * multiplier);
            }

            _loopScheduled = true;
            SendCustomEventDelayedSeconds(nameof(_SlowUpdate), UpdateInterval);
        }
    }
}
