using UnityEngine;

namespace BlockEngine.Objects.Core
{
    public class Corner
    {
        internal readonly IntVector2[] BLOCK_OFFSETS = new IntVector2[4] { new IntVector2(-1, -1), new IntVector2(-1, 0), new IntVector2(0, 0), new IntVector2(0, -1) };

        private readonly BlockEngine _blockEngine;
        private readonly IntVector2 _gridPosition;

        private readonly Vector2 initianlPosition;


        internal Vector2 Position { get; set; }
        internal bool HasBackground { get; private set; }

        public float WaterPressure { get; private set; }
        public Corner(BlockEngine blockEngine, IntVector2 gridPosition)
        {
            this._blockEngine = blockEngine;
            this._gridPosition = gridPosition;
            Position = gridPosition;
            initianlPosition = gridPosition;
        }

        internal Corner GetCorner(Direction direction)
        {
            return _blockEngine.GetCorner(_gridPosition + direction.Offset());
        }

        internal Block GetBlock(IntVector2 offset)
        {
            return _blockEngine.GetBlock(_gridPosition + offset);
        }

        public Block GetBaseBlock()
        {
            return _blockEngine.GetBlock(_gridPosition);
        }



    }

}