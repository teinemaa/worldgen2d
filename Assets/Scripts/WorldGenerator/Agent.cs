using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockEngine;
using UnityEngine;
using Random = System.Random;
using Scripts.WorldGenerator;

namespace Assets.Scripts.WorldGenerator
{
    [Serializable]
    public abstract class Agent : MonoBehaviour
    {
        public int agentType = 1;   //1 = normal agent, 2 = ore agent

        public abstract IntVector2 GetRadius(Random numGen);
        
        public abstract void Run(AgentManipulator input, Random numGen);


    }
}
