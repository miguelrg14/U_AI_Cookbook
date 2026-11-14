// Miguel Rodriguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Entity escaping from another entity when near, action logic
    /// </summary>
    public class ActionEscape : IAction
    {
        GameObject entity_gameObject;
        FsmEntityStats entityStats;
        Rigidbody rb;
        VisualFeedback visualFeedback;
        TargetDetector targetDetector;


        [Header("Movement values")]
        public float rotationSpeed = 500;
        public float escapeSpeed = 500;
        public float maxSpeed = 10f;

        void Start()
        {
            entityStats = GetComponentInParent<FsmEntityStats>();
            rb = GetComponentInParent<Rigidbody>();
            visualFeedback = GetComponentInParent<VisualFeedback>();
            targetDetector = GetComponentInParent<TargetDetector>();
        }
        /// <summary>
        /// The function sets the hiding variable to false, sets the velocity of the rigidbody to zero & activates visual feedback
        /// </summary>
        public override void Act()
        {
            EscapeOppositeDirection(targetDetector.target, escapeSpeed);
            entityStats.isPanicking = false;
            entityStats.isEscaping = true;

            visualFeedback.stateImage_escaping.gameObject.SetActive(true);
            visualFeedback.stateImage_chilling.gameObject.SetActive(false);
            visualFeedback.stateImage_lookingOut.gameObject.SetActive(false);
            visualFeedback.stateImage_resting.gameObject.SetActive(false);
        }
        /// <summary>
        ///     Escape opposite direction of target logic
        /// </summary>
        void EscapeOppositeDirection(GameObject target, float speed)
        {
            entity_gameObject = rb.gameObject;

            if (target == null)
                return;

            entityStats.UseStamina();
            AgentMotor.MoveAwayFrom(rb, target.transform.position, speed, maxSpeed, rotationSpeed);
        }
    }
}
