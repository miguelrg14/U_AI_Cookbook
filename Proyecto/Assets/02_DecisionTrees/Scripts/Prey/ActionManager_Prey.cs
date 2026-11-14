// Miguel Rodr�guez Gallego
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AICookbook.DecisionTrees
{
    using AICookbook.Shared;


    /// <summary>
    ///     Manages action methods of action tree logic of Prey
    /// </summary>
    public class ActionManager_Prey : MonoBehaviour
    {
        EntityStats stats;
        TargetDetector_Prey targetDetector;
        [HideInInspector] public Rigidbody rb;

        [SerializeField] GameObject target;

        [Header("Visual state feedback")]
        [SerializeField] Image stateImage_escaping;
        [SerializeField] Image stateImage_chilling;
        [SerializeField] Image stateImage_lookingOut;
        [SerializeField] Image stateImage_resting;

        /// <summary>
        ///     Movement values towards Citizen - Zombie => Chase - Escape
        /// </summary>
        [Header("Movement values")]
        public float rotationSpeed = 500;
        public float movementSpeed = 500;
        public float escapeSpeed = 500;
        public float maxSpeed = 10f;
        [SerializeField] bool randmRotateOnStart = false;

        /// <summary>
        ///     Idle wandering when there is nothing else to do
        /// </summary>
        readonly WanderRoutine wander = new WanderRoutine();

        void Awake()
        {
            stats = GetComponent<EntityStats>();
            rb = GetComponent<Rigidbody>();
            targetDetector = GetComponent<TargetDetector_Prey>();
        }
        void Update()
        {
            target = targetDetector.target;
        }
        /// <summary>
        ///     Escape opposite direction of target logic
        /// </summary>
        void EscapeOppositeDirection(GameObject target, float speed)
        {
            stats.UseStamina();
            AgentMotor.MoveAwayFrom(rb, target.transform.position, speed, maxSpeed, rotationSpeed);
        }
        /// <summary>
        ///     Wanders around while idle
        /// </summary>
        void ChillRoutineLogic() => wander.Tick(rb, movementSpeed);


        /// <summary>
        ///     If prey sees hunter, it runs away from him on opposite direction
        /// </summary>
        public void Escape()
        {
            target = targetDetector.target;
            if (target == null || !target.activeInHierarchy) return;

            EscapeOppositeDirection(target, escapeSpeed);

            stateImage_escaping.gameObject.SetActive(true);
            stateImage_chilling.gameObject.SetActive(false);
            stateImage_lookingOut.gameObject.SetActive(false);
            stateImage_resting.gameObject.SetActive(false);
        }
        /// <summary>
        ///     While there's no hunter in the area, it doesn't do anything
        /// </summary>
        public void Chill()
        {
            ChillRoutineLogic();

            stateImage_escaping.gameObject.SetActive(false);
            stateImage_chilling.gameObject.SetActive(true);
            stateImage_lookingOut.gameObject.SetActive(false);
            stateImage_resting.gameObject.SetActive(false);
        }
        /// <summary>
        ///     Lookout when hunter is very near, so prey stops and looks towards it
        /// </summary>
        public void LookOut()
        {
            // Stop first: whatever happens next, a prey on the look-out must not keep moving
            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Look-out happens when the hunter is seen (see range) but is not close yet (look-out range),
            // so the hunter to watch is the one in the see range
            GameObject seen = targetDetector.target;
            if (seen != null && seen.activeInHierarchy)
                AgentMotor.FacePosition(transform, seen.transform.position, rotationSpeed);

            stateImage_escaping.gameObject.SetActive(false);
            stateImage_chilling.gameObject.SetActive(false);
            stateImage_lookingOut.gameObject.SetActive(true);
            stateImage_resting.gameObject.SetActive(false);
        }
        /// <summary>
        ///     Hunter doesn't have stamina to follow prey
        ///     Played when (stamina == 0) and till it is fully recovered
        /// </summary>
        public void Rest()
        {
            rb.linearVelocity = Vector3.zero;
            stats.RegenStamina();

            stateImage_escaping.gameObject.SetActive(false);
            stateImage_chilling.gameObject.SetActive(false);
            stateImage_lookingOut.gameObject.SetActive(false);
            stateImage_resting.gameObject.SetActive(true);
        }
    }
}
