using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    public class ConditionNoStamina : ICondition
    {
        FsmEntityStats stats;

        void Awake()
        {
            stats = GetComponentInParent<FsmEntityStats>();
        }

        /// <summary>
        ///     Test if entity has no stamina
        /// </summary>
        public override bool Test()
        {
            if (stats.GetStamina() <= 0)
                return true;

            else
                return false;
        }
    }
}
