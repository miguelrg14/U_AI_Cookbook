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

        protected GameObject closeTarget;

        public int CloseTargetRange => closeTargetRange;

        public override void FindTarget()
        {
            base.FindTarget();

            closeTarget = FindClosest(closeTargetRange);
            if (closeTarget != null)
                lastPositionSeen = closeTarget.transform.position;
        }
    }
}
