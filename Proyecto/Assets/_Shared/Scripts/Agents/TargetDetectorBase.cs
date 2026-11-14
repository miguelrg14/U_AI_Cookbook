// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Finds every frame the closest object of <see cref="targetLayerMask"/> inside the see range.
    /// </summary>
    public abstract class TargetDetectorBase : MonoBehaviour
    {
        [HideInInspector] public GameObject target;
        [HideInInspector] public Vector3 lastPositionSeen;

        [Header("Detection parameters")]
        [SerializeField] protected LayerMask targetLayerMask;
        [Tooltip("Targets inside this range are seen")]
        [SerializeField] protected int seeTargetRange = 20;

        /// <summary>
        ///     True for agents that detect their own kind, so they don't detect themselves
        /// </summary>
        protected virtual bool IgnoreSelf => false;

        public int SeeTargetRange => seeTargetRange;

        protected virtual void OnEnable() => AIDebugView.Register(this);
        protected virtual void OnDisable() => AIDebugView.Unregister(this);

        protected virtual void Update() => FindTarget();

        public virtual void FindTarget()
        {
            target = FindClosest(seeTargetRange);
            if (target != null)
                lastPositionSeen = target.transform.position;
        }

        /// <summary>
        ///     Closest collider of the target layers inside <paramref name="range"/>, or null
        /// </summary>
        protected GameObject FindClosest(float range)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, range, targetLayerMask);
            GameObject closest = null;
            float closestSqrDistance = float.PositiveInfinity;
            Vector3 position = transform.position;

            foreach (Collider hit in hits)
            {
                if (IgnoreSelf && hit.gameObject == gameObject)
                    continue;

                float sqrDistance = (hit.transform.position - position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = hit.gameObject;
                }
            }
            return closest;
        }

        /// <summary>
        ///     Ranges in the Scene view when the agent is selected
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = AIDebugView.SeeRangeColor;
            Gizmos.DrawWireSphere(transform.position, seeTargetRange);
            if (target != null)
                Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }
}
