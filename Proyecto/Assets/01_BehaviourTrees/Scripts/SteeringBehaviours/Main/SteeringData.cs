using UnityEngine;

namespace AICookbook.BehaviourTrees
{

    public class SteeringData
    {
        public Vector3 linear;
        public float angular;

        public SteeringData()
        {
            linear = Vector3.zero;
            angular = 0f;
        }
    }
}
