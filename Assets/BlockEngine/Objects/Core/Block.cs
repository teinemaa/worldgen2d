using System.Collections.Generic;
using BlockEngine.Objects.Core;
using UnityEngine;
using System.Collections;

namespace BlockEngine
{
    public class Block
    {

        internal static readonly IntVector2[] CORNER_OFFSETS = new IntVector2[4] { IntVector2.ZERO, IntVector2.UP, IntVector2.ONE, IntVector2.RIGHT };

        private readonly BlockEngine blockEngine;
        private readonly IntVector2 gridPosition;
        internal BlockProperties BlockProperties { get; set; }
        internal BackgroundProperties BackgroundProperties { get; set; }
        internal bool IsEmpty { get { return BlockProperties == BlockProperties.EMPTY; } }


        public Vector2 Position
        {
            get
            {
                Vector2 sum = Vector2.zero;
                foreach (IntVector2 offset in CORNER_OFFSETS)
                {
                    sum += GetCorner(offset).Position;
                }
                return 0.25f*sum;
            }
        }


        internal Block(BlockEngine blockEngine, IntVector2 gridPosition, BlockProperties blockProperties, BackgroundProperties backgroundProperties)
        {
            this.blockEngine = blockEngine;
            this.gridPosition = gridPosition;
            this.BlockProperties = blockProperties;
            this.BackgroundProperties = backgroundProperties;
        }


        internal Corner GetCorner(IntVector2 offset)
        {
            return blockEngine.GetCorner(gridPosition + offset);
        }


        internal bool HasBackground { get { return BackgroundProperties != BackgroundProperties.EMPTY; } }


        public Block GetBlock(Direction direction)
        {
            return blockEngine.GetBlock(gridPosition + direction.Offset());
        }



    }
}
 