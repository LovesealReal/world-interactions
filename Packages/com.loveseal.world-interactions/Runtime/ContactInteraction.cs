// World Interactions — contact-to-Udon relay | by Loveseal

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Bridges one avatar contact tag to creator code. Senders are local-only, so a contact is
    /// only detected on the broadcaster's client. When Synced, that client owns this object and
    /// syncs the state; every client then applies the effect while its player is within
    /// EffectRange of the broadcaster (the object's owner).
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ContactInteraction : UdonSharpBehaviour
    {
        private const float RangeCheckInterval = 0.25f;

        [HideInInspector] public string InteractionName;
        [HideInInspector] public UdonSharpBehaviour Target;
        [HideInInspector] public string StartEvent;
        [HideInInspector] public string EndEvent;
        [HideInInspector] public bool Synced;
        [HideInInspector] public float EffectRange; // meters around the broadcaster; 0 = entire instance
        [HideInInspector] public bool ExemptPresence;
        [HideInInspector] public PresenceDetector Detector;

        /// <summary>True while the broadcast is active on this client (before range/exemption).</summary>
        [HideInInspector] public bool IsActive;

        [UdonSynced] private bool _syncedActive;
        private int _localContacts;
        private bool _effectOn;
        private bool _rangeLoopScheduled;

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
            if (Synced)
            {
                TakeOwnership();
                _syncedActive = active;
                RequestSerialization();
            }
            SetBroadcastActive(active);
        }

        public override void OnDeserialization()
        {
            if (!Synced) return;

            // Re-assert if another broadcaster switched it off while our contact is still live.
            if (!_syncedActive && _localContacts > 0)
            {
                TakeOwnership();
                _syncedActive = true;
                RequestSerialization();
                return;
            }

            SetBroadcastActive(_syncedActive);
        }

        // Clear stale state if the broadcaster left and ownership fell to us.
        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!Synced || player == null || !player.isLocal) return;
            if (_syncedActive && _localContacts == 0)
            {
                _syncedActive = false;
                RequestSerialization();
                SetBroadcastActive(false);
            }
        }

        private void TakeOwnership()
        {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        private void SetBroadcastActive(bool active)
        {
            if (active == IsActive) return;
            IsActive = active;

            if (!active)
            {
                SetEffect(false);
            }
            else if (Synced && EffectRange > 0f)
            {
                if (!_rangeLoopScheduled) _RangeCheck();
            }
            else
            {
                SetEffect(true);
            }
        }

        public void _RangeCheck()
        {
            _rangeLoopScheduled = false;
            if (!IsActive || !Synced || EffectRange <= 0f) return;

            SetEffect(IsLocalPlayerInRange());
            _rangeLoopScheduled = true;
            SendCustomEventDelayedSeconds(nameof(_RangeCheck), RangeCheckInterval);
        }

        private bool IsLocalPlayerInRange()
        {
            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            VRCPlayerApi local = Networking.LocalPlayer;
            if (!Utilities.IsValid(owner) || !Utilities.IsValid(local)) return false;
            if (owner.isLocal) return true;
            return Vector3.Distance(owner.GetPosition(), local.GetPosition()) <= EffectRange;
        }

        private void SetEffect(bool on)
        {
            if (on == _effectOn) return;

            if (on)
            {
                if (ExemptPresence && Detector != null && Detector.IsLocalPlayerBroadcasting) return;
                _effectOn = true;
                if (Target != null && !string.IsNullOrEmpty(StartEvent)) Target.SendCustomEvent(StartEvent);
            }
            else
            {
                _effectOn = false;
                if (Target != null && !string.IsNullOrEmpty(EndEvent)) Target.SendCustomEvent(EndEvent);
            }
        }
    }
}
