using Assets.Scripts.WorldGenerator;
using Assets.Scripts.WorldGenerator.Agents;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;
using System.Collections;
using Random = System.Random;

public class BSPDungeonAgent : Agent {

    public IntVector2 minRadius;
    public IntVector2 maxRadius;
    private IntVector2 myRadius;

    public int minRoomSize;
    public int maxRoomSize;

    public override IntVector2 GetRadius(Random numGen)
    {
        // TODO: initialize this publically
        minRadius = new IntVector2(12, 12);
        maxRadius = new IntVector2(500, 500);

        minRoomSize = 7;
        maxRoomSize = 20;

        myRadius = new IntVector2(numGen.Next(minRadius.x, maxRadius.x + 1),
                                    numGen.Next(minRadius.y, maxRadius.y + 1));
        if (myRadius.x > myRadius.y - 1)
            myRadius = new IntVector2(myRadius.y, myRadius.y);

        return myRadius;
    }

    public override void Run(AgentManipulator input, Random numGen)
    {
        int height = myRadius.y;
        int width = myRadius.x / 2;

        // only below surface
        if (input.GetBlockType(new IntVector2(0, -1)) == BlockType.Empty || 
            input.LocalToWorldPos(input.LocalZero).y >= 0) // TODO: this should actually check the groundline
            return;

        if (input.GetBlockType(new IntVector2(0, 0)) == BlockType.Wood ||
            input.GetBlockType(new IntVector2(0, 0)) == BlockType.Leaves)
            return;

        //input.LocalZero = new IntVector2(input.LocalZero.x - myRadius.x, input.LocalZero.y - myRadius.y);
        BSPNode.CreateNodes(input, input.LocalZero, myRadius, minRoomSize, maxRoomSize, numGen);
    }

}
