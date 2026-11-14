// Miguel Rodr�guez Gallego
using UnityEngine;

namespace AICookbook.BehaviourTrees
{
    using AICookbook.Shared;


    /// <summary>
    ///     Citizen movement depending on if there's a threat or not nearby
    ///         2 tipes of movement mainly: 
    ///             - Chill Routine
    ///             - Escape
    /// </summary>
    public class CitizenMovement : MonoBehaviour
    {
        TargetDetector_Citizen targetDetector;
        Rigidbody rb;

        /// <summary>
        ///     Target to be looking at
        /// </summary>
        [SerializeField] GameObject target;

        /// <summary>
        ///     Movement values towards Citizen - Zombie => Chase - Escape
        /// </summary>
        [Header("Movement values")]
        public float rotationSpeed = 500;
        public float movementSpeed = 200;
        public float maxSpeed = 10f;
        [SerializeField] bool randmRotateOnStart = false;


        /// <summary>
        ///     Idle wandering when there is nothing else to do
        /// </summary>
        readonly WanderRoutine wander = new WanderRoutine();

        void Start()
        {
            if (randmRotateOnStart)
                transform.Rotate(transform.up * Random.Range(-180, 180));
        }
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            targetDetector = GetComponent<TargetDetector_Citizen>();
        }
        /// <summary>
        ///     Target - Action control methods
        /// </summary>
        void Update()
        {
            target = GetComponent<TargetDetector_Citizen>().target;

            if (target == null)
            {
                ChillRoutineLogic();
                return;
            }

            MoveOnOppositeDirectionToZombie();
        }

        /// <summary>
        ///     Escape from closer zombie moving on the oposite direction
        /// </summary>
        public void MoveOnOppositeDirectionToZombie()
        {
            wander.Restart();
            AgentMotor.MoveAwayFrom(rb, target.transform.position, movementSpeed, maxSpeed, rotationSpeed);
        }

        /// <summary>
        ///     Wanders around while idle
        /// </summary>
        void ChillRoutineLogic() => wander.Tick(rb, movementSpeed);
    }
}
