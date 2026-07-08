// World Interactions — contact-to-Udon relay | by Loveseal

using UdonSharp;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDKBase;

namespace Loveseal.WorldInteractions
{
    public enum FalloffMode
    {
        None = 0,   // full intensity anywhere in range
        Linear = 1, // fades linearly to 0 at the range edge
        Smooth = 2, // smoothstep fade
    }

    public enum RangeMode
    {
        Players = 0,      // events fire per player as they enter/leave the range
        WorldObjects = 1, // events fire while broadcasting; query range per object via GetIntensity
    }

    /// <summary>
    /// Bridges one avatar contact tag to creator code. Senders are local-only, so a contact is
    /// only detected on the broadcaster's client. When Synced, that client owns this object and
    /// syncs the state; effects are ranged around the broadcaster (the owner). Before the start
    /// event, the relay assigns itself to a 'SourceInteraction' variable on the target (if
    /// declared) so handlers can read LocalIntensity or call GetIntensity.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ContactInteraction : UdonSharpBehaviour
    {
        private const float TickInterval = 0.25f;
        private const string SourceVariable = "SourceInteraction";

        [HideInInspector] public string InteractionName;
        [HideInInspector] public UdonSharpBehaviour Target;
        [HideInInspector] public string StartEvent;
        [HideInInspector] public string EndEvent;
        [HideInInspector] public bool Synced;
        [HideInInspector] public float EffectRange; // meters around the broadcaster; 0 = unlimited
        [HideInInspector] public FalloffMode Falloff;
        [HideInInspector] public RangeMode Mode;
        [HideInInspector] public bool ExemptPresence;
        [HideInInspector] public PresenceDetector Detector;

        /// <summary>True while the broadcast is active on this client.</summary>
        [HideInInspector] public bool IsActive;

        /// <summary>Falloff intensity (0-1) at the local player, updated every tick while active.</summary>
        [HideInInspector] public float LocalIntensity;

        /// <summary>Broadcaster position, updated every tick while active.</summary>
        [HideInInspector] public Vector3 BroadcasterPosition;

        [UdonSynced] private bool _syncedActive;
        private int _localContacts;
        private bool _effectOn;
        private bool _tickScheduled;

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
                LocalIntensity = 0f;
                SetEffect(false);
                return;
            }
            if (!_tickScheduled) _Tick();
        }

        // Tracks the broadcaster and gates Players-mode effects by range while active.
        public void _Tick()
        {
            _tickScheduled = false;
            if (!IsActive) return;

            VRCPlayerApi owner = Networking.GetOwner(gameObject);
            VRCPlayerApi local = Networking.LocalPlayer;
            if (Utilities.IsValid(owner) && Utilities.IsValid(local))
            {
                BroadcasterPosition = owner.GetPosition();
                LocalIntensity = GetIntensity(local.GetPosition());
            }
            else
            {
                LocalIntensity = 0f;
            }

            SetEffect(Mode == RangeMode.WorldObjects || LocalIntensity > 0f);

            _tickScheduled = true;
            SendCustomEventDelayedSeconds(nameof(_Tick), TickInterval);
        }

        /// <summary>Falloff intensity (0-1) at a world position, relative to the broadcaster.</summary>
        public float GetIntensity(Vector3 worldPosition)
        {
            if (!IsActive) return 0f;
            if (EffectRange <= 0f) return 1f;

            float distance = Vector3.Distance(worldPosition, BroadcasterPosition);
            if (distance > EffectRange) return 0f;

            float t = 1f - distance / EffectRange;
            if (Falloff == FalloffMode.Linear) return t;
            if (Falloff == FalloffMode.Smooth) return t * t * (3f - 2f * t);
            return 1f;
        }

        private void SetEffect(bool on)
        {
            if (on == _effectOn) return;

            if (on)
            {
                if (ExemptPresence && Detector != null && Detector.IsLocalPlayerBroadcasting) return;
                _effectOn = true;
                if (Target != null)
                {
                    Target.SetProgramVariable(SourceVariable, this);
                    if (!string.IsNullOrEmpty(StartEvent)) Target.SendCustomEvent(StartEvent);
                }
            }
            else
            {
                _effectOn = false;
                if (Target != null && !string.IsNullOrEmpty(EndEvent)) Target.SendCustomEvent(EndEvent);
            }
        }
    }
}
