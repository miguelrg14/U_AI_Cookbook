// Miguel Rodriguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Check if target 
    /// </summary>
    public class ConditionTargetActive : ICondition
    {
        TargetDetector targetDetector;

        [SerializeField] GameObject target;

        void Awake()
        {
            targetDetector = GetComponentInParent<TargetDetector>();
        }

        public override bool Test()
        {
            target = targetDetector.target;

            if (target != null)
                return true;

            else
                return false;
        }
    }
}
