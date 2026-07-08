// World Interactions — built-in Slow effect
// by Loveseal | v1.0.0

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace ClubMaul.WorldInteractions
{
    /// <summary>
    /// Built-in handler for the Staff Scanner's 'Slow' world feature. While active, the local
    /// player's walk/run/strafe speeds are multiplied by <see cref="SpeedMultiplier"/>; the
    /// original speeds are captured on activation and restored afterwards.
    /// Driven by a ContactInteraction relay via OnSlowStart/OnSlowEnd.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SlowEffect : UdonSharpBehaviour
    {
        [Tooltip("Movement speed is multiplied by this while Slow is active.")]
        [Range(0f, 1f)]
        public float SpeedMultiplier = 0.5f;

        private bool _applied;
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

            player.SetWalkSpeed(_walkSpeed * SpeedMultiplier);
            player.SetRunSpeed(_runSpeed * SpeedMultiplier);
            player.SetStrafeSpeed(_strafeSpeed * SpeedMultiplier);
            _applied = true;
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
    }
}
