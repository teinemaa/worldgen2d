using System;
using System.Collections.Generic;
using System.Linq;
using BlockEngine.Objects.Core;
using UnityEngine;

namespace BlockEngine.Objects.Junks
{
    public class JunkLoader
    {

        public const float MaximumBeamLength = 5f;
        public const int JunkSize = 400;

        private const int HorizontalJunks = 2000;
        private const int VerticalJunks = 500;
        private const int HalfWorldWidth = HorizontalJunks * JunkSize / 2;
        private const int HalfWorldHeight = VerticalJunks * JunkSize / 2;

        private readonly Junk[,] _junks;
        private readonly BlockEngine _blockEngine;
        private readonly BlockWorldGenerator _worldGenerator;

        public JunkLoader(BlockEngine blockEngine)
        {
            _blockEngine = blockEngine;
            _worldGenerator = _blockEngine.blockWorldGenerator;
            _junks = new Junk[HorizontalJunks, VerticalJunks];
        }

        public Corner GetCorner(IntVector2 gridPosition)
        {
            IntVector2 index = GridPositionToIndex(gridPosition);
            return GetJunk(index).GetCorner(index);
        }

        public Corner CreateCorner(IntVector2 index)
        {
            return new Corner(_blockEngine, IndexToGridPosition(index));
        }

        public Block GetBlock(IntVector2 gridPosition)
        {
            IntVector2 index = GridPositionToIndex(gridPosition);
            return GetJunk(index).GetBlock(index);
        }

        public Block CreateBlock(IntVector2 index)
        {
            IntVector2 gridPosition = IndexToGridPosition(index);
            BlockProperties blockProperties = _worldGenerator.GetBlockProperties(gridPosition.x, gridPosition.y);
            BackgroundProperties backgroundProperties = _worldGenerator.GetBackgroundProperties(gridPosition.x, gridPosition.y);
            return new Block(_blockEngine, IndexToGridPosition(index), blockProperties, backgroundProperties);
        }


        public IEnumerable<Corner> GetCurrentCorners(Rect area, bool expand)
        {
            if (expand)
            {
                area.Set(area.xMin - MaximumBeamLength, area.yMin - MaximumBeamLength, area.width + 2 * MaximumBeamLength, area.height + 2 * MaximumBeamLength);
            }
            IntVector2 minIndex = new IntVector2(Mathf.FloorToInt(area.min.x), Mathf.FloorToInt(area.min.y));
            IntVector2 maxIndex = new IntVector2(Mathf.CeilToInt(area.max.x), Mathf.CeilToInt(area.max.y));
            for (int i = minIndex.x; i <= maxIndex.x; i++)
            {
                for (int j = minIndex.y; j < maxIndex.y; j++)
                {
                    IntVector2 pocketIndex = new IntVector2(i, j);
                    yield return GetCorner(new IntVector2(i, j));
                }
            }
        }


        private Junk GetJunk(IntVector2 index)
        {
            int x = index.x / JunkSize, y = index.y / JunkSize;
            return _junks[x, y] ?? (_junks[x, y] = new Junk(this));
        }

        private IntVector2 GridPositionToIndex(IntVector2 gridPosition)
        {
            return new IntVector2(gridPosition.x + HalfWorldWidth, gridPosition.y + HalfWorldHeight);
        }

        private IntVector2 IndexToGridPosition(IntVector2 index)
        {
            return new IntVector2(index.x - HalfWorldWidth, index.y - HalfWorldHeight);
        }


    }



}
