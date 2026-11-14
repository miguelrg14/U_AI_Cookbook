// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.DecisionTrees
{
    using AICookbook.Shared;

    /// <summary>
    ///     Prey detection for the hunter: it sees prey in the outer range and can attack
    ///     the closest one inside the attack range.
    /// </summary>
    public class TargetDetector_Hunter : TwoRangeTargetDetector
    {
        /// <summary>
        ///     Closest target inside the close range
        /// </summary>
        public GameObject attackableTarget => closeTarget;
    }
}
