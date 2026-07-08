// World Interactions — presence beacon detector | by Loveseal

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Detects the local player's presence beacon (e.g. a staff badge). Beacons are local-only
    /// senders, so only the wearer's own client ever hears one — a reliable "is this player
    /// badged?" check. The receiver on this object is added at build time.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PresenceDetector : UdonSharpBehaviour
    {
        /// <summary>True while the local player broadcasts a presence tag.</summary>
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
