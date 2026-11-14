// Miguel Rodriguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Check if target isn't in sight / in target detector list<> 
    /// </summary>
    public class ConditionTargetOutOfSight : ICondition
    {
        TargetDetector targetDetector;

        [SerializeField] GameObject target;
        [SerializeField] GameObject closeTarget;

        void Awake()
        {
            targetDetector = GetComponentInParent<TargetDetector>();
        }

        /// <summary>
        ///     Tests if there's no target
        /// </summary>
        public override bool Test()
        {
            target = targetDetector.target;
            closeTarget = targetDetector.veryCloseTarget;

            if ((target == null && closeTarget == null) || target == null)
            {
                //if (targetDetector.gameObject.layer == LayerMask.GetMask("Prey"))
                //    targetDetector.gameObject.GetComponent<EntityStats>().isPanicking = false;

                return true;
            }

            else
                return false;
        }
    }
}
