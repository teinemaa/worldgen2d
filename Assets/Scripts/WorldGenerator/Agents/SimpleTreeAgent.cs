using Assets.Scripts.WorldGenerator;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;
using System.Collections;
using Random = System.Random;

public class SimpleTreeAgent : Agent
{
    public IntVector2 minRadius;
    public IntVector2 maxRadius;

    private IntVector2 myRadius;

    public int distToOtherTree;

    public override IntVector2 GetRadius(Random numGen)
    {
        // TODO: initialize this publically
        distToOtherTree = 4;
        

        return maxRadius;
    }

    public override void Run(AgentManipulator input, Random numGen)
    {

        minRadius = new IntVector2(3, 2);
        maxRadius = new IntVector2(5, 7);
        myRadius = new IntVector2(numGen.Next(minRadius.x, maxRadius.x + 1),
                                    numGen.Next(minRadius.y, maxRadius.y + 1));
        if (myRadius.x > myRadius.y - 1)
            myRadius = new IntVector2(myRadius.y, myRadius.y);

        int height = myRadius.y;
        int width = myRadius.x/2;
        
        // only on surface
        if (input.GetBlockType(new IntVector2(0, -myRadius.y-1)) == BlockType.Empty ||
            input.GetBlockType(new IntVector2(0, -myRadius.y-1)) == BlockType.Cave)
            //input.LocalToWorldPos(new IntVector2(0, -myRadius.y)).y < 0) // TODO: this should actually check the groundline
            return;

        // there should be enough light at the root ;)
        if (input.GetBlockType(new IntVector2(1, 0)) != BlockType.Empty ||
            input.GetBlockType(new IntVector2(-1, 0)) != BlockType.Empty)
            return;

        // no tree should be below the new tree
        if (input.GetBlockType(new IntVector2(0, -myRadius.y - 2)) == BlockType.Wood ||
            input.GetBlockType(new IntVector2(0, -myRadius.y - 1)) == BlockType.Leaves)
            return;

        // nothing should be above
        if (input.GetBlockType(new IntVector2(0, myRadius.y)) != BlockType.Empty)
            return;

        // no tree should be in up to x distance
        for (int i = -distToOtherTree; i <= distToOtherTree; i++)
        {
            if (i != 0 && 
                (input.GetBlockType(new IntVector2(i, myRadius.y)) == BlockType.Wood ||
                input.GetBlockType(new IntVector2(i, -myRadius.y)) == BlockType.Wood))
                return;
        }
        
        // create tree
        for (int i = -myRadius.y; i < myRadius.y-1; i++)
        {
            for (int j = -width; j <= width; j++)
            {
                if(j != 0 && i > 0)
                    input.SetBlockType(new IntVector2(j, i), BlockType.Leaves);
                if(j == 0)
                    input.SetBlockType(new IntVector2(j, i), BlockType.Wood, true);
            }
        }
        for (int j = -width + 1; j <= width - 1; j++)
        {
            input.SetBlockType(new IntVector2(j, myRadius.y - 1), BlockType.Leaves);
        }

        input.Finish();
    }


}
