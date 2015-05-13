using Assets.Scripts.WorldGenerator;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;
using System.Collections;
using Random = System.Random;

public class RainAgent : Agent {
    public override IntVector2 GetRadius(Random numGen)
    {
        throw new System.NotImplementedException();
    }

    public override void Run(AgentManipulator input, Random numGen)
    {
        // return if underground
        if (input.LocalZero.y < 0 || input.GetBlockType(new IntVector2(0, -1)) != BlockType.Empty) // TODO: actually check the ground line
            return;

        // TODO drop water
    }

}
