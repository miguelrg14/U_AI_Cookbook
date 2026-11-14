// Miguel Rodr�guez Gallego
using UnityEngine;

namespace AICookbook.BehaviourTrees
{
    using AICookbook.Shared;


    /// <summary>
    ///     Zombie movement depending on if there's a citizen to infect nearby or not
    ///         2 tipes of movement mainly: 
    ///             - Chill Routine
    ///             - Chase
    /// </summary>
    public class ZombieMovement : MonoBehaviour, IAIDebugLabel
    {
        TargetDetector_Zombie targetDetectorZombie;
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
        public float movementSpeed = 500;
        public float maxSpeed = 10f;
        [SerializeField] bool randmRotateOnStart = false;

        /// <summary>
        ///     Prefab to instantiate when infecting a citizen
        /// </summary>
        [Header("Infection Info")]
        [SerializeField] GameObject zombiePrefab;

        /// <summary>
        ///     Idle wandering when there is nothing else to do
        /// </summary>
        readonly WanderRoutine wander = new WanderRoutine();

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            targetDetectorZombie = GetComponent<TargetDetector_Zombie>();
        }
        void Start()
        {
            if (randmRotateOnStart)
                transform.Rotate(transform.up * Random.Range(-180, 180));
        }
        /// <summary>
        ///     Target - Action control methods
        /// </summary>
        /// <summary>
        ///     What the agent is doing this frame (shown by the AI debug view)
        /// </summary>
        string currentBehaviour;

        void OnEnable() => AIDebugView.Register(this);
        void OnDisable() => AIDebugView.Unregister(this);

        public Vector3 LabelPosition => transform.position + Vector3.up * 2.2f;
        public string DebugLabel => currentBehaviour;

        void Update()
        {
            target = GetComponent<TargetDetector_Zombie>().target;

            if (target == null)
            {
                currentBehaviour = "Wander";
                ChillRoutineLogic();
                return;
            }

            currentBehaviour = "Chase citizen";
            MoveTowardsCitizen();        
        }

        /// <summary>
        ///     Infect citizen when zombie collides with it
        /// </summary>
        void OnCollisionEnter(Collision collision)
        {
            ConvertToZombie(collision.collider.gameObject);
        }
        void ConvertToZombie(GameObject hit)
        {
            if (hit.tag == "Citizen")
            {
                Destroy(hit);
                Instantiate(zombiePrefab, hit.transform.position, hit.transform.rotation, hit.transform.parent);
                target = null;  // To eliminate it from the collider[] targets list and search for another 
            }
        }

        /// <summary>
        ///     Make zombie move directly towards citizen
        /// </summary>
        public void MoveTowardsCitizen()
        {
            AgentMotor.MoveTowards(rb, target.transform.position, movementSpeed, maxSpeed, rotationSpeed);
        }


        /// <summary>
        ///     Wanders around while idle
        /// </summary>
        void ChillRoutineLogic() => wander.Tick(rb, movementSpeed);
    }
}
