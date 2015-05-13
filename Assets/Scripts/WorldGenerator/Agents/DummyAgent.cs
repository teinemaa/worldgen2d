using System;
using System.Collections.Generic;
using System.Linq;
using BlockEngine;
using Scripts.WorldGenerator;

namespace Assets.Scripts.WorldGenerator.Agents
{
    public class DummyAgent : Agent
    {
        public override IntVector2 GetRadius(Random numGen)
        {
            return new IntVector2(3, 3);
        }

        public override void Run(AgentManipulator input, Random numGen)
        {
     

            input.SetBlockType(IntVector2.ZERO, BlockType.Sand);
/*
            foreach (IntVector2 neighbour in neighbours(input.LocalZero).Where(neighbour => numGen.Next(10) < 4))
            {
                input.SetBlockType(neighbour, BlockType.Sand);
            }
            */
            input.Finish();
        }

        private static IEnumerable<IntVector2> neighbours(IntVector2 origin)
        {
            var ret = new List<IntVector2>
            {
                new IntVector2(origin.x + 1, origin.y),
                new IntVector2(origin.x - 1, origin.y),
                new IntVector2(origin.x, origin.y + 1),
                new IntVector2(origin.x, origin.y - 1),
                new IntVector2(origin.x + 1, origin.y + 1),
                new IntVector2(origin.x - 1, origin.y + 1),
                new IntVector2(origin.x + 1, origin.y - 1),
                new IntVector2(origin.x - 1, origin.y - 1)
            };
            return ret;
        }


    }
}