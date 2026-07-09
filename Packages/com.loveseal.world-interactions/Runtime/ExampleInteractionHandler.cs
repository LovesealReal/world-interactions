// World Interactions — example custom interaction handler | by Loveseal
//
// Starting point for your own interactions: write public methods, add the behaviour to your
// scene, then reference it from a Custom Interaction entry on the World Interactions component.

using UdonSharp;
using UnityEngine;

namespace Loveseal.WorldInteractions
{
    /// <summary>Toggles a GameObject while its interaction is active.</summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ExampleInteractionHandler : UdonSharpBehaviour
    {
        [Tooltip("Object turned on while the interaction is active.")]
        public GameObject TargetObject;

        public void OnInteractionStart()
        {
            if (TargetObject != null) TargetObject.SetActive(true);
        }

        public void OnInteractionEnd()
        {
            if (TargetObject != null) TargetObject.SetActive(false);
        }
    }
}
