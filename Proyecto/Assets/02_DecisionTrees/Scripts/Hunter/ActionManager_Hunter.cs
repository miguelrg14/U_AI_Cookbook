// Miguel Rodr�guez Gallego
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AICookbook.DecisionTrees
{
    using AICookbook.Shared;


    /// <summary>
    ///     Manages action methods of action tree logic of Hunter
    /// </summary>
    public class ActionManager_Hunter : MonoBehaviour
    {
        EntityStats stats;
        TargetDetector_Hunter targetDetector;
        [HideInInspector] public Rigidbody rb;
        [SerializeField] Vector3 preyLastPosition;

        [SerializeField] GameObject target;
        [SerializeField] GameObject attackableTarget;

        //[Header("Parameters (spawn another entity after killing it)")]
        //[SerializeField] GameObject preyPrefab;
        //[SerializeField] Transform[] spawnPoints;
        //[SerializeField] GameObject entityTransformParent;

        [Header("Visual state feedback")]
        [SerializeField] Image stateImage_attacking;
        [SerializeField] Image stateImage_following;
        [SerializeField] Image stateImage_searching;
        [SerializeField] Image stateImage_resting;

        /// <summary>
        ///     Movement values towards Citizen - Zombie => Chase - Escape
        /// </summary>
        [Header("Movement values")]
        public float rotationSpeed = 500;
        public float movementSpeed = 500;
        public float attackSpeed = 500;
        public float followSpeed = 500;
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
            targetDetector = GetComponent<TargetDetector_Hunter>();
        }
        void Update()
        {
            target = targetDetector.target;
            attackableTarget = targetDetector.attackableTarget;
            preyLastPosition = targetDetector.lastPositionSeen;
        }

        /// <summary>
        ///     When hunter collides prey, he attacks/kills it!
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Prey"))
            {
                Kill(collision.gameObject);
                rb.linearVelocity = Vector3.zero;
                //Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
                //Instantiate(preyPrefab, spawn).transform.parent = entityTransformParent.transform;            
            }
        }

        /// <summary>
        ///     Wanders around while idle
        /// </summary>
        void ChillRoutineLogic() => wander.Tick(rb, movementSpeed);
        /// <summary>
        ///     Regens stamina automatically with time
        /// </summary>
        void MoveTowards(GameObject target, float speed)
        {
            // A prey killed this frame is still referenced until the detector updates
            if (target == null || !target.activeInHierarchy) return;

            stats.UseStamina();
            AgentMotor.MoveTowards(rb, target.transform.position, speed, maxSpeed, rotationSpeed);
        }

        /// <summary>
        ///     Attack prey / destroy it
        /// </summary>
        public void Attack()
        {
            attackableTarget = targetDetector.attackableTarget;
            MoveTowards(attackableTarget, attackSpeed);
            stateImage_attacking.gameObject.SetActive(true);
            stateImage_following.gameObject.SetActive(false);
            stateImage_searching.gameObject.SetActive(false);
            stateImage_resting.gameObject.SetActive(false);
        }
        /// <summary>
        ///     Removes the prey the hunter has touched (not the one it was following, which can be another)
        /// </summary>
        public void Kill(GameObject prey)
        {
            if (prey == null) return;

            prey.SetActive(false);
        }
        /// <summary>
        ///     When prey is on sight, hunter follows it till it gets to its position
        /// </summary>
        public void Follow()
        {
            target = targetDetector.target;
            MoveTowards(target, followSpeed);
            stateImage_attacking.gameObject.SetActive(false);
            stateImage_following.gameObject.SetActive(true);
            stateImage_searching.gameObject.SetActive(false);
            stateImage_resting.gameObject.SetActive(false);
        }
        /// <summary>
        ///     Search for target while isn't on sight
        /// </summary>
        public void Search()
        {
            ChillRoutineLogic();
            stateImage_attacking.gameObject.SetActive(false);
            stateImage_following.gameObject.SetActive(false);
            stateImage_searching.gameObject.SetActive(true);
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
            stateImage_attacking.gameObject.SetActive(false);
            stateImage_following.gameObject.SetActive(false);
            stateImage_searching.gameObject.SetActive(false);
            stateImage_resting.gameObject.SetActive(true);
        }
    }
}
