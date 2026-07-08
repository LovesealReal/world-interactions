// World Interactions — contact-to-Udon relay
// by Loveseal | v1.0.0

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Bridges one avatar contact tag to creator code. The VRCContactReceiver on this object
    /// (added by the build-time generator) fires OnContactEnter/OnContactExit; this relay
    /// tracks the active state and sends the configured custom events to the target behaviour.
    ///
    /// Because compatible senders are local-only, a contact is only ever detected on the
    /// client of the player whose avatar sent it. With InstanceWide on, that client takes
    /// ownership and syncs the state so every client (late joiners included) runs the events;
    /// off, the events run only on the detecting client.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ContactInteraction : UdonSharpBehaviour
    {
        [HideInInspector] public string InteractionName;
        [HideInInspector] public UdonSharpBehaviour Target;
        [HideInInspector] public string StartEvent;
        [HideInInspector] public string EndEvent;
        [HideInInspector] public bool InstanceWide;
        [HideInInspector] public bool ExemptPresence;
        [HideInInspector] public PresenceDetector Detector;

        /// <summary>True while this interaction is active on this client (before presence exemption).</summary>
        [HideInInspector] public bool IsActive;

        [UdonSynced] private bool _syncedActive;
        private int _localContacts;
        private bool _handlerNotified;

        public override void OnContactEnter(ContactEnterInfo contactInfo)
        {
            _localContacts++;
            if (_localContacts == 1) OnLocalStateChanged(true);
        }

        public override void OnContactExit(ContactExitInfo contactInfo)
        {
            if (_localContacts > 0) _localContacts--;
            if (_localContacts == 0) OnLocalStateChanged(false);
        }

        private void OnLocalStateChanged(bool active)
        {
            if (InstanceWide)
            {
                TakeOwnership();
                _syncedActive = active;
                RequestSerialization();
            }
            Dispatch(active);
        }

        public override void OnDeserialization()
        {
            if (!InstanceWide) return;

            // Another activator switched the synced state off while our own contact is still
            // live (e.g. two broadcasters toggled the same feature) — re-assert it.
            if (!_syncedActive && _localContacts > 0)
            {
                TakeOwnership();
                _syncedActive = true;
                RequestSerialization();
                return;
            }

            Dispatch(_syncedActive);
        }

        // If ownership falls to us (e.g. the activating player left mid-effect) and we can't
        // see the contact ourselves, clear the stale synced state for everyone.
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!InstanceWide || player == null || !player.isLocal) return;
            if (_syncedActive && _localContacts == 0)
            {
                _syncedActive = false;
                RequestSerialization();
                Dispatch(false);
            }
        }

        private void TakeOwnership()
        {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        private void Dispatch(bool active)
        {
            if (active == IsActive) return;
            IsActive = active;

            if (active)
            {
                if (ExemptPresence && Detector != null && Detector.IsLocalPlayerBroadcasting) return;
                _handlerNotified = true;
                if (Target != null && !string.IsNullOrEmpty(StartEvent)) Target.SendCustomEvent(StartEvent);
            }
            else
            {
                // Only send the end event if the start event actually fired on this client.
                if (!_handlerNotified) return;
                _handlerNotified = false;
                if (Target != null && !string.IsNullOrEmpty(EndEvent)) Target.SendCustomEvent(EndEvent);
            }
        }
    }
}
