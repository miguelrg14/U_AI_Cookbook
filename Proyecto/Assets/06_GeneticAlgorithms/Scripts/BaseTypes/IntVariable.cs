using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICookbook.GeneticAlgorithms
{

    [CreateAssetMenu(menuName = "Variables/Int")]
    public class IntVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        public int InitialValue;

        [NonSerialized]
        public int RuntimeValue;

        public void OnAfterDeserialize()
        {
            RuntimeValue = InitialValue;
        }

        public void OnBeforeSerialize() { }
    }
}
