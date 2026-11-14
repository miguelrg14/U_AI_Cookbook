// Miguel Rodríguez Gallego
using UnityEngine;
using UnityEngine.Serialization;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Target detector with a second, closer range:
    ///     <list type="bullet">
    ///         <item><see cref="TargetDetectorBase.target"/>: closest target inside the see range (outer)</item>
    ///         <item><see cref="closeTarget"/>: closest target inside the close range (look out, attack...)</item>
    ///     </list>
    ///     Subclasses expose the close target with the name that makes sense for each agent.
    /// </summary>
    public abstract class TwoRangeTargetDetector : TargetDetectorBase
    {
        [Tooltip("Second range used for closer checks (look out, attack, panic...)")]
        [FormerlySerializedAs("lookOutTargetRange")]
        [FormerlySerializedAs("canAttackTargetRange")]
        [FormerlySerializedAs("seeClosestTargetsRange")]
        [SerializeField] protected int closeTargetRange = 15;
        [Tooltip("Hysteresis: once a target is in the close range, it stays the close target until it is " +
                 "this much farther than the range. Avoids flickering between two decisions at the border.")]
        [SerializeField] protected float closeRangeExitMargin = 3f;

        protected GameObject closeTarget;

        public int CloseTargetRange => closeTargetRange;
        public float CloseRangeExitDistance => closeTargetRange + closeRangeExitMargin;
        public GameObject CloseTarget => closeTarget;

        public override void FindTarget()
        {
            base.FindTarget();

            GameObject newCloseTarget = FindClosest(closeTargetRange);

            // Keep the current close target while it is inside the exit distance, even if it just left the range
            if (newCloseTarget == null && IsStillClose(closeTarget))
                newCloseTarget = closeTarget;

            closeTarget = newCloseTarget;
            if (closeTarget != null)
                lastPositionSeen = closeTarget.transform.position;
        }

        bool IsStillClose(GameObject current)
        {
            if (current == null || !current.activeInHierarchy) return false;
            float exit = CloseRangeExitDistance;
            return (current.transform.position - transform.position).sqrMagnitude <= exit * exit;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = AIDebugView.CloseRangeColor;
            Gizmos.DrawWireSphere(transform.position, closeTargetRange);
            if (closeTarget != null)
                Gizmos.DrawLine(transform.position, closeTarget.transform.position);
        }
    }
}
