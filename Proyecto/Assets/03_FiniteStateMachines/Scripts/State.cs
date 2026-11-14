// Miguel Rodriguez Gallego
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     State main info
    /// </summary>
    public class State : MonoBehaviour
    {
        StatesManager statesManager;

        public bool isActive = false;

        [Header("Action 1")]
        public IAction action;               // Action to make if state is activated
        public List<Transition> transitions;    // Transitions to check

        void Awake()
        {
            statesManager = GetComponentInParent<StatesManager>();
            action = GetComponent<IAction>();
            transitions.AddRange(GetComponentsInChildren<Transition>().ToArray());
        }
    }
}
