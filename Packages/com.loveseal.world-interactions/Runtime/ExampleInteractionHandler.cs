// World Interactions — example custom interaction handler
// by Loveseal | v1.0.0
//
// Copy this as a starting point for your own interactions:
//   1. Write an UdonSharpBehaviour with a public method per event you care about.
//   2. Add it to any object in your scene.
//   3. On the World Interactions component, add a Custom Interaction with your collision
//      tag, drag your behaviour into Target, and put your method names in Start/End Event.
// The matching contact receiver is generated at build time; when an avatar broadcasts your
// tag, your methods run.

using UdonSharp;
using UnityEngine;

namespace Loveseal.WorldInteractions
{
    /// <summary>Toggles a GameObject while its interaction is active — e.g. a light, a
    /// particle system, or a whole effects rig.</summary>
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
