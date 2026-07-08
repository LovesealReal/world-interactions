// World Interactions — proximity object toggler | by Loveseal

using UdonSharp;
using UnityEngine;

namespace Loveseal.WorldInteractions
{
    /// <summary>
    /// Generic handler that enables each target object while it's inside the broadcaster's
    /// range (e.g. lights that flicker near them). Use as a Custom Interaction target with
    /// Range Mode set to World Objects and the default OnInteractionStart/End events.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ProximityObjectToggler : UdonSharpBehaviour
    {
        private const float CheckInterval = 0.25f;

        [Tooltip("Objects enabled while within the broadcaster's range.")]
        public GameObject[] Targets;

        [HideInInspector] public ContactInteraction SourceInteraction; // injected by the relay

        private bool _active;
        private bool _loopScheduled;

        public void OnInteractionStart()
        {
            _active = true;
            if (!_loopScheduled) _ProximityCheck();
        }

        public void OnInteractionEnd()
        {
            _active = false;
        }

        public void _ProximityCheck()
        {
            _loopScheduled = false;
            if (!_active)
            {
                SetAll(false);
                return;
            }

            if (Targets != null)
            {
                foreach (GameObject target in Targets)
                {
                    if (target == null) continue;
                    bool on = SourceInteraction == null ||
                              SourceInteraction.GetIntensity(target.transform.position) > 0f;
                    target.SetActive(on);
                }
            }

            _loopScheduled = true;
            SendCustomEventDelayedSeconds(nameof(_ProximityCheck), CheckInterval);
        }

        private void SetAll(bool on)
        {
            if (Targets == null) return;
            foreach (GameObject target in Targets)
                if (target != null) target.SetActive(on);
        }
    }
}
