#region

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.WorldGenerator;
using BlockEngine;
using WorldGeneration.Core;

#endregion

namespace Scripts.WorldGenerator
{
    public class WorldGeneratorChunk
    {
        public static readonly IntVector2 chunkSize = new IntVector2(50, 50);
        public bool Generated { get; private set; }

        public IEnumerable<VoronoiPoint> VoronoiPoints
        {
            get { return _baseVoronoiPoints; }
        }

        // functions to be used for chunk-wide cellular automata runs (in that order)
        public List<Func<List<BlockType>, BlockType>> cellularFuncs;
        public int cellularNeighbourLevel = 1;
        public int maxVoronoiPoints = 200;
        public int minVoronoiPoints = 70;

        private readonly WorldGeneratorGlobalFunctions _globalFunctions;
        private readonly IntVector2 _index;
        private readonly IntVector2 _maxBoundExclusive; // 
        private readonly IntVector2 _minBoundInclusive; // global
        private readonly Random _random; // chunk number generator
        private readonly int _seed; // global seed
        private readonly int[] fillTypeWeights = {5, 3, 1, 1};
        private VoronoiPoint[] _baseVoronoiPoints;
        private Biome[,] _biomeInformation;
        private BlockType[,] _chunkData; // the blocks to be generated
        private bool[,] _chunkLockData; // true = block at pos(x,y) is locked and cant be overwritten until generation


        public WorldGeneratorChunk(WorldGeneratorGlobalFunctions globalFunctions, int seed, IntVector2 index)
        {
            _globalFunctions = globalFunctions;
            _seed = seed;
            _index = index;
            _random = new Random(GetHashCode());
            _minBoundInclusive = index*chunkSize;
            _maxBoundExclusive = (index + IntVector2.ONE)*chunkSize;

            GenerateVoronoiPoints();
            Generated = false;

            // TODO do this dynamically for different layers
            cellularFuncs = new List<Func<List<BlockType>, BlockType>>
            {
                RandomValue,
                RandomValue,
                RandomValue, 
                MostAppearingValue,
                MostAppearingValue
            };
        }

        public void FillGround(List<VoronoiPoint> neighbourVoronoiPoints, Biome[] biomes)
        {
            ResolveVoronoi(neighbourVoronoiPoints, biomes);

            foreach (Func<List<BlockType>, BlockType> func in cellularFuncs)
            {
                RunCellularStep(func);
            }

            RunAllAgents();
        }

        public List<BlockType> GetNeighboursOfLevel(int level, IntVector2 curPos,
            Direction lastDirection = Direction.Down, Boolean rec = false)
        {
            List<BlockType> ret = new List<BlockType>();

            foreach (Direction direction in DirectionSupport.VALUES)
            {
                if (rec && direction == lastDirection) continue;

                IntVector2 neighbourPos = curPos + direction.Offset();

                if (neighbourPos.x < 0 || neighbourPos.y < 0 ||
                    neighbourPos.x >= chunkSize.x ||
                    neighbourPos.y >= chunkSize.y)
                    continue;

                ret.Add(_chunkData[neighbourPos.x, neighbourPos.y]);
                if (level > 1)
                    ret.AddRange(GetNeighboursOfLevel(level - 1, curPos, direction, true));
            }

            return ret;
        }

        public BlockType GetWorldBlockIndex(IntVector2 worldPosition)
        {
            IntVector2 localIndex = worldPosition - _minBoundInclusive;
            return _chunkData[localIndex.x, localIndex.y];
        }

        public BlockType GetChunkBlockIndex(IntVector2 chunkPos)
        {
            return _chunkData[chunkPos.x, chunkPos.y];
        }

        public void SetChunkBlockIndex(IntVector2 chunkPos, BlockType type)
        {
            if (chunkPos.x < 0 || chunkPos.y < 0 ||
                chunkPos.x >= chunkSize.x ||
                chunkPos.y >= chunkSize.y)
                return;

            _chunkData[chunkPos.x, chunkPos.y] = type;
        }

        public bool GetChunkBlockLock(IntVector2 chunkPos)
        {
            return _chunkLockData[chunkPos.x, chunkPos.y];
        }

        public void SetChunkBlockLock(IntVector2 chunkPos, bool lockState)
        {
            _chunkLockData[chunkPos.x, chunkPos.y] = lockState;
        }

        public IntVector2 ChunkToWorldPos(IntVector2 chunkPos)
        {
            return _minBoundInclusive + chunkPos;
        }

        public IntVector2 WorldToChunkPos(IntVector2 worldPos)
        {
            return worldPos - _minBoundInclusive;
        }

        protected bool Equals(WorldGeneratorChunk other)
        {
            return _index.Equals(other._index) && _seed == other._seed;
        }

        private void RunAllAgents()
        {
            for (int i = 0; i < _chunkData.GetLength(0); i++)
            {
                for (int j = 0; j < _chunkData.GetLength(1); j++)
                {
                    Agent agent = null;//Biome.AgentWeight.GetRandomComponent()_biomeInformation[i, j].GetAgent(_random);*/
                    if (agent != null)
                    {
                        switch (agent.agentType)
                        {
                            case 2:
                                RunAgent(agent, i, j, BlockType.Ore);
                                break;
                            default:
                                RunAgent(agent, i, j);
                                break;
                        }
                    }
                }
            }
        }

        private void RunAgent(Agent agent, int x, int y)
        {
            IntVector2 radius = agent.GetRadius(_random);
            if (x - radius.x < 0 || y - radius.y < 0 ||
                x + radius.x >= chunkSize.x || y + radius.y >= chunkSize.y)
                return;

            //agent.Run(new AgentManipulator(this, new IntVector2(x, y)), _random);
        }

        private void RunAgent(Agent agent, int x, int y, BlockType type)
        {
            IntVector2 radius = agent.GetRadius(_random);
            if (x - radius.x < 0 || y - radius.y < 0 ||
                x + radius.x >= chunkSize.x || y + radius.y >= chunkSize.y)
                return;


            int size = radius.x*radius.y;
            double variance = 0.1;

            //agent.Run(new AgentManipulator(this, new IntVector2(x, y)), _random, type, size, variance);
        }

        private void RunCellularStep(Func<List<BlockType>, BlockType> func)
        {
            BlockType[,] tempData = new BlockType[_chunkData.GetLength(0), _chunkData.GetLength(1)];

            for (int i = 0; i < _chunkData.GetLength(0); i++)
            {
                for (int j = 0; j < _chunkData.GetLength(1); j++)
                {
                    IntVector2 pos = new IntVector2(i, j);
                    BlockType toBlock = _chunkData[i, j];

                    List<BlockType> neighbours = GetNeighboursOfLevel(cellularNeighbourLevel, pos);

                    tempData[i, j] = func(neighbours);
                }
            }
            _chunkData = tempData;
        }

        private BlockType RandomValue(List<BlockType> blocks)
        {
            return blocks[_random.Next(blocks.Count)];
        }

        /**
		 * initialize chunk, set voronoi regions for biomes in relation to neighbour chunks
		 */

        private void ResolveVoronoi(List<VoronoiPoint> neighbourVoronoiPoints, Biome[] biomes)
        {
            Generated = true;
            _chunkData = new BlockType[chunkSize.x, chunkSize.y];
            _chunkLockData = new bool[chunkSize.x, chunkSize.y];
            _biomeInformation = new Biome[chunkSize.x, chunkSize.y];
            List<VoronoiPoint> allVoronoiPoints = new List<VoronoiPoint>();
            allVoronoiPoints.AddRange(_baseVoronoiPoints);
            allVoronoiPoints.AddRange(neighbourVoronoiPoints);

            for (int i = 0; i < _chunkData.GetLength(0); i++)
            {
                for (int j = 0; j < _chunkData.GetLength(1); j++)
                {
                    IntVector2 target = _minBoundInclusive + new IntVector2(i, j);
                    VoronoiPoint closestVoronoiPoint =
                        allVoronoiPoints.Aggregate(
                            (minItem, nextItem) =>
                                (target - minItem.WorldPosition).SqrMagnitude <
                                (target - nextItem.WorldPosition).SqrMagnitude
                                    ? minItem
                                    : nextItem);
                    _chunkData[i, j] = closestVoronoiPoint.BlockType;
                    _chunkLockData[i, j] = false;
                    _biomeInformation[i, j] = biomes[0];
                }
            }
        }

        // TODO should be decided based on layers and mineral, not in voronoi

        private void GenerateVoronoiPoints()
        {
            int nrOfVoronoiPoints = _random.Next(minVoronoiPoints, maxVoronoiPoints);
            _baseVoronoiPoints = new VoronoiPoint[nrOfVoronoiPoints];
            BlockType[] blockTypes = (BlockType[]) Enum.GetValues(typeof (BlockType));
            for (int i = 0; i < nrOfVoronoiPoints; i++)
            {
                int x = _random.Next(_minBoundInclusive.x, _maxBoundExclusive.x);
                int y = _random.Next(_minBoundInclusive.y, _maxBoundExclusive.y);

                BlockType blockType = BlockType.Empty;
                bool aboveGround = WorldGeneratorGlobalFunctions.IsAboveGround(x, y);
                if (aboveGround)
                {
                    blockType = BlockType.Empty;
                }
                else
                {
                    int total = fillTypeWeights.Sum();
                    int fillTypeIndex = _random.Next(total);
                    for (int j = 0; j < fillTypeWeights.Length; j++)
                    {
                        fillTypeIndex -= fillTypeWeights[j];
                        if (fillTypeIndex < 0)
                        {
                            blockType = (BlockType) (j - 1);
                            break;
                        }
                    }
                }
                _baseVoronoiPoints[i] = new VoronoiPoint(new IntVector2(x, y), blockType);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((WorldGeneratorChunk) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_index.GetHashCode()*397) ^ _seed;
            }
        }

        private static BlockType MostAppearingValue(List<BlockType> blocks)
        {
            BlockType queue = blocks
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(1)
                .Select(g => g.Key).ToArray()[0];
            return queue;
        }



        private static int AmountOfState(List<int> blocks, int state)
        {
            IEnumerable<int> query = from num in blocks where num == state select num;
            return query.Count();
        }
    }
}
