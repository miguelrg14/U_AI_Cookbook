// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;

    /// <summary>
    ///     Detects agents of the same kind (other prey), skipping itself, so the group
    ///     can react when a colleague panics or escapes.
    /// </summary>
    public class TargetDetector_Colleague : TwoRangeTargetDetector
    {
        /// <summary>
        ///     Closest target inside the close range
        /// </summary>
        public GameObject veryCloseTarget => closeTarget;

        protected override bool IgnoreSelf => true;
    }
}
