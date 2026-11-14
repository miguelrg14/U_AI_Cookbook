// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.DecisionTrees
{
    using AICookbook.Shared;

    /// <summary>
    ///     Hunter detection for the prey: it sees hunters in the outer range and keeps watching
    ///     the closest one in the look-out range.
    /// </summary>
    public class TargetDetector_Prey : TwoRangeTargetDetector
    {
        /// <summary>
        ///     Closest target inside the close range
        /// </summary>
        public GameObject watchingTarget => closeTarget;
    }
}
