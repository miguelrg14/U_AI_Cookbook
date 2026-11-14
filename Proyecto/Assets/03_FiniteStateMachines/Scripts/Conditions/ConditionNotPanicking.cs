// Miguel Rodríguez Gallego
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Detects if entity is not panicking
    /// </summary>
    public class ConditionNotPanicking : ICondition
    {
        EntityStats entityStats;

        void Awake()
        {
            entityStats = GetComponentInParent<EntityStats>();
        }

        /// <summary>
        ///     Tests if entity is not panicking
        /// </summary>
        public override bool Test()
        {
            if (!entityStats.isPanicking)
            {
                return true;
            }

            else
                return false;
        }
    }
}
