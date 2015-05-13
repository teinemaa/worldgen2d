using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockEngine;
using Scripts.WorldGenerator;

namespace Assets.Scripts.WorldGenerator
{
    public class VoronoiPoint
    {
        public IntVector2 WorldPosition { get; private set; }
        public BlockType BlockType { get; private set; }

        public VoronoiPoint(IntVector2 worldPosition, BlockType blockType)
        {
            WorldPosition = worldPosition;
            BlockType = blockType;
        }
    }
}
