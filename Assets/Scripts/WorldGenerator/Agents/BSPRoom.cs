using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;

namespace Assets.Scripts.WorldGenerator.Agents
{
    public class BSPRoom
    {
        public IntVector2 topLeft;
        public IntVector2 dimensions;

        public int left {
            get { return topLeft.x; }
        }
        public int right
        {
            get { return topLeft.x + dimensions.x; }
        }
        public int top
        {
            get { return topLeft.y; }
        }
        public int bottom
        {
            get { return topLeft.y + dimensions.y; }
        }

        public BSPRoom(IntVector2 topLeft, IntVector2 dimensions)
        {
            this.topLeft = topLeft;
            this.dimensions = dimensions;
        }

        public void Draw(AgentManipulator input)
        {
            for (int i = 0; i < dimensions.x; i++)
            {
                for (int j = 0; j < dimensions.y; j++)
                {
                    IntVector2 drawPos = topLeft + new IntVector2(i, j);
                    BlockType toDraw = BlockType.Cave;

                    if (i == 0)
                    {
                        if(j == 0 || input.GetBlockType(new IntVector2(drawPos.x - 1, drawPos.y)) != BlockType.StoneBrick)
                                toDraw = BlockType.StoneBrick;
                    }
                    else if (i == dimensions.x-1)
                    {
                        if(j == dimensions.y-1 || input.GetBlockType(new IntVector2(drawPos.x + 1, drawPos.y)) != BlockType.StoneBrick)
                            toDraw = BlockType.StoneBrick;
                    }
                    else if (j == 0)
                    {
                        if(input.GetBlockType(new IntVector2(drawPos.x, drawPos.y - 1)) != BlockType.StoneBrick)
                                toDraw = BlockType.StoneBrick;
                    }
                    else if (j == dimensions.y - 1)
                    {
                        if(input.GetBlockType(new IntVector2(drawPos.x, drawPos.y + 1)) != BlockType.StoneBrick)
                                toDraw = BlockType.StoneBrick;
                    }

                    try
                    {
                        input.SetBlockType(drawPos, toDraw, true);
                    }
                    catch (BlockIsLockedException e)
                    {
                        return;
                    }
                }
            }
        }
    }
}
