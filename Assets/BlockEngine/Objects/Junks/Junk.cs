using System.Collections.Generic;
using BlockEngine.Objects.Core;

namespace BlockEngine.Objects.Junks
{
    public class Junk
    {
        private const int JunkSize = JunkLoader.JunkSize;

        private readonly Block[,] _blocks;
        private readonly Corner[,] _corners;
        private readonly JunkLoader _junkLoader;

        public Junk(JunkLoader junkLoader)
        {
            _junkLoader = junkLoader;
            _corners = new Corner[JunkSize, JunkSize];
            _blocks = new Block[JunkSize, JunkSize];
        }

        public Corner GetCorner(IntVector2 index)
        {
            int x = index.x%JunkSize, y = index.y%JunkSize;
            return _corners[x, y] ?? (_corners[x, y] = _junkLoader.CreateCorner(index));
        }

        public Block GetBlock(IntVector2 index)
        {
            int x = index.x%JunkSize, y = index.y%JunkSize;
            Block block = _blocks[x, y];
            if (block == null)
            {
                block = _junkLoader.CreateBlock(index);
                _blocks[x, y] = block;
            }
            return block;
        }

    }
}