// Miguel Rodriguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Entity wandering & chilling around when nobody around, action logic
    /// </summary>
    public class ActionChill : IAction
    {
        GameObject entity_gameObject;
        FsmEntityStats entityStats;
        Rigidbody rb;
        VisualFeedback visualFeedback;

        /// <summary>
        ///     Movement values towards Citizen - Zombie => Chase - Escape
        /// </summary>
        [Header("Movement values")]
        public float rotationSpeed = 500;
        public float movementSpeed = 500;
        public float attackSpeed = 500;
        public float maxSpeed = 10f;
        [SerializeField] bool randmRotateOnStart = false;

        /// <summary>
        ///     Idle wandering when there is nothing else to do
        /// </summary>
        readonly WanderRoutine wander = new WanderRoutine();


        void Start()
        {
            entityStats = GetComponentInParent<FsmEntityStats>();
            rb = GetComponentInParent<Rigidbody>();
            visualFeedback = GetComponentInParent<VisualFeedback>();
        }
        /// <summary>
        ///     Visual feedback depending on entity & action logic
        /// </summary>
        public override void Act()
        {
            if (entityStats.gameObject.tag == "Hunter")
            {
                visualFeedback.stateImage_attacking.gameObject.SetActive(false);
                visualFeedback.stateImage_following.gameObject.SetActive(false);
                visualFeedback.stateImage_searching.gameObject.SetActive(true);
                visualFeedback.stateImage_resting.gameObject.SetActive(false);
            }
            if (entityStats.gameObject.tag == "Prey")
            {
                entityStats.isEscaping = false;

                visualFeedback.stateImage_escaping.gameObject.SetActive(false);
                visualFeedback.stateImage_chilling.gameObject.SetActive(true);
                visualFeedback.stateImage_lookingOut.gameObject.SetActive(false);
                visualFeedback.stateImage_resting.gameObject.SetActive(false);
            }

            ChillRoutineLogic();
        }

        /// <summary>
        ///     Wanders around while idle
        /// </summary>
        void ChillRoutineLogic() => wander.Tick(rb, movementSpeed);
    }
}
