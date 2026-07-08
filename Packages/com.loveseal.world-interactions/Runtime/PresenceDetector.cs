// World Interactions — presence beacon detector
// by Loveseal | v1.0.0

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Listens for presence-beacon broadcasts (e.g. a staff badge like the Club Maul Staff
    /// Scanner). Compatible beacons are local-only senders, so this receiver can only ever hear
    /// the LOCAL player's own beacon — which makes it a reliable "is the person at this keyboard
    /// wearing the badge?" check. Read <see cref="IsLocalPlayerBroadcasting"/> from your own
    /// UdonSharp code, or let ContactInteraction's Exempt Presence option use it for you.
    /// The VRCContactReceiver on this object is added by the build-time generator.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PresenceDetector : UdonSharpBehaviour
    {
        /// <summary>True while the local player is broadcasting one of the presence tags.</summary>
        [HideInInspector] public bool IsLocalPlayerBroadcasting;

        private int _activeContacts;

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            _activeContacts++;
            IsLocalPlayerBroadcasting = true;
        }

        public override void OnContactExit(ContactExitInfo contactInfo)
        {
            _activeContacts--;
            if (_activeContacts <= 0)
            {
                _activeContacts = 0;
                IsLocalPlayerBroadcasting = false;
            }
        }
    }
}
