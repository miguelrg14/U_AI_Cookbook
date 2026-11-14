// Miguel Rodriguez Gallego
using UnityEngine;

namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;


    /// <summary>
    ///     Logic of prey when has a hunter in vision range, that makes him stop and wait for him to leave
    /// </summary>
    public class ActionLookOut : IAction
    {
        GameObject entity_gameObject;
        FsmEntityStats entityStats;
        Rigidbody rb;
        VisualFeedback visualFeedback;
        TargetDetector targetDetector;

        [Header("Movement values")]
        public float rotationSpeed = 500;

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
            entityStats.isPanicking = false;
            entityStats.isEscaping = false;

            entity_gameObject = rb.gameObject;

            if (targetDetector.target == null)
                return;

            AgentMotor.FacePosition(entity_gameObject.transform, targetDetector.target.transform.position, rotationSpeed);

            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            visualFeedback.stateImage_escaping.gameObject.SetActive(false);
            visualFeedback.stateImage_chilling.gameObject.SetActive(false);
            visualFeedback.stateImage_lookingOut.gameObject.SetActive(true);
            visualFeedback.stateImage_resting.gameObject.SetActive(false);
        }
    }
}
