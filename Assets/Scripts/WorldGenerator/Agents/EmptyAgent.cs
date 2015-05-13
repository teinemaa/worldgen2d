using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockEngine;

namespace Assets.Scripts.WorldGenerator.Agents
{
    class EmptyAgent : Agent
    {
        public override IntVector2 GetRadius(Random numGen)
        {
            return IntVector2.ZERO;
        }

        public override void Run(AgentManipulator input, Random numGen)
        {
            
        }

    }
}
