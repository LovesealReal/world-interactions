// World Interactions — staff scanner presence detector
// by Loveseal | v1.0.0

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;

namespace ClubMaul.WorldInteractions
{
    /// <summary>
    /// Listens for the Staff Scanner's presence beacon. The scanner's senders are local-only,
    /// so this receiver can only ever hear the LOCAL player's own scanner — which makes it a
    /// reliable "is the person at this keyboard staff?" check. Read <see cref="IsLocalPlayerStaff"/>
    /// from your own UdonSharp code, or let ContactInteraction's Exempt Staff option use it for you.
    /// The VRCContactReceiver on this object is added by the build-time generator.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class StaffScannerDetector : UdonSharpBehaviour
    {
        /// <summary>True while the local player is broadcasting a Staff Scanner presence tag.</summary>
        [HideInInspector] public bool IsLocalPlayerStaff;

        private int _activeContacts;

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            _activeContacts++;
            IsLocalPlayerStaff = true;
        }

        public override void OnContactExit(ContactExitInfo contactInfo)
        {
            _activeContacts--;
            if (_activeContacts <= 0)
            {
                _activeContacts = 0;
                IsLocalPlayerStaff = false;
            }
        }
    }
}
