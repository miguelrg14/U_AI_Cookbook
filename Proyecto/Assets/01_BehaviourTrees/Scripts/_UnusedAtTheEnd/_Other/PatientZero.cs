using UnityEngine;

namespace AICookbook.BehaviourTrees
{

    [RequireComponent(typeof(Entity))]
    public class PatientZero : MonoBehaviour
    {
        public string virusName;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Entity>().SetInfection(new Virus(virusName, 0));
        }
    }
}
