using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICookbook.GeneticAlgorithms
{

    [CreateAssetMenu(menuName = "Variables/Player List")]
    public class PlayerList : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<PlayerInfo> Players;
   

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
       
        }
    }
}
