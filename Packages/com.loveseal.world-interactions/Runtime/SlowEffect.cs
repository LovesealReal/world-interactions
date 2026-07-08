// World Interactions — built-in Slow effect | by Loveseal

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Scales the local player's walk/run/strafe speed while active; original speeds are
    /// captured on start and restored on end. Driven by a ContactInteraction relay.
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
