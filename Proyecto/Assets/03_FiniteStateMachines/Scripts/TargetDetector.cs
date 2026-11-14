// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;

    /// <summary>
    ///     Detects the other kind of agent (prey for hunters, hunters for prey) in two ranges.
    /// </summary>
    public class TargetDetector : TwoRangeTargetDetector
    {
        /// <summary>
        ///     Closest target inside the close range
        /// </summary>
        public GameObject veryCloseTarget => closeTarget;
    }
}
