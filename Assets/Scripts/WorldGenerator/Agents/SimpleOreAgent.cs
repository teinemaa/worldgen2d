using System;
using Assets.Scripts.WorldGenerator;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;
using System.Collections;
using Random = System.Random;
using System.Collections.Generic;
public class SimpleOreAgent : Agent
{

    #region VarDeclaration

    public IntVector2 minRadius;
    public IntVector2 maxRadius;

    private IntVector2 myRadius;

    public int distToOtherVeins;
    public double chanceToSpawn = 0.5;

    #endregion


    #region AgentMembers

    public override IntVector2 GetRadius(Random numGen)
    {
        // TODO: initialize this publically

        return myRadius;
    }

 

    public override void Run(AgentManipulator input, Random numGen)
    {
        if (input.GetBlockType(IntVector2.ZERO) == BlockType.Stone ||
            (input.GetBlockType(IntVector2.ZERO) == BlockType.Dirt && numGen.NextDouble() > 0.8f))
        {
            List<IntVector2> currentOres = new List<IntVector2>();
            currentOres.Add(IntVector2.ZERO);
            input.SetBlockType(IntVector2.ZERO, BlockType.Ore);
            int totalOres = numGen.Next(2, 9);
            for (int i = 0; i < totalOres; i++)
            {
                IntVector2 ore = currentOres[numGen.Next(currentOres.Count)];
                IntVector2 newOre = ore + DirectionSupport.PRIMARY_DIRECTIONS[numGen.Next(4)].Offset();
                input.SetBlockType(newOre, BlockType.Ore);
            }

            input.Finish();
        }

    }
    #endregion
}

public class Vein
{
    public int x, y;
    private int currentX;
    private int currentY;

    public Vein(int currentX, int currentY)
    {
        // TODO: Complete member initialization
        this.currentX = currentX;
        this.currentY = currentY;
    }
}
