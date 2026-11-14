// Miguel Rodriguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Entity action when paniquing (when other entity of same type is near escaping)
    /// </summary>
    public class ActionPanic : IAction
    {
        GameObject entity_gameObject;
        FsmEntityStats entityStats;
        Rigidbody rb;
        VisualFeedback visualFeedback;
        TargetDetector_Colleague targetDetector;

        [Header("Movement values")]
        public float rotationSpeed = 500;
        public float escapeSpeed = 500;
        public float maxSpeed = 10f;

        Vector3 targetLastPosition;

        void Start()
        {
            entityStats = GetComponentInParent<FsmEntityStats>();
            rb = GetComponentInParent<Rigidbody>();
            visualFeedback = GetComponentInParent<VisualFeedback>();
            targetDetector = GetComponentInParent<TargetDetector_Colleague>();
        }
        /// <summary>
        /// The function sets the hiding variable to false, sets the velocity of the rigidbody to zero & activates visual feedback
        /// </summary>
        public override void Act()
        {
            if (targetDetector.target == null)
                entityStats.isPanicking = false;

            EscapeOppositeDirection(targetDetector.target, escapeSpeed);

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

            entityStats.isPanicking = true;
            entityStats.isEscaping = false;

            if (target != null)
                targetLastPosition = target.transform.position;

            entityStats.UseStamina();
            AgentMotor.MoveAwayFrom(rb, targetLastPosition, speed, maxSpeed, rotationSpeed);
        }
    }
}
