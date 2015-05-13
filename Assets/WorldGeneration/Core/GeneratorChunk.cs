#region

using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.WorldGenerator;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;

#endregion

namespace WorldGeneration.Core
{
    public sealed class GeneratorChunk
    {
        public enum State
        {
            Initialized,
            BiomePoints,
            ClusterPoints,
            Filled,
            CellularAutomaton,
            AgentsCenter,
            AgentsUp,
            AgentsUpRight,
            AgentsRight,
            AgentsLeft,
            ReadyToTriggerNeighbours,
            Done
        }

        public IEnumerable<ClusterEntry> ClusterEntries { get; private set; }
        public IEnumerable<BiomeEntry> BiomeEntries { get; private set; }


        public readonly IntVector2 ChunkIndex;
        public State CurrentState = State.Initialized;
        public int cellularNeighbourLevel = 1;

        private readonly GeneratorChunkLoader _chunkLoader;
        private readonly int _chunkSize;
        private readonly Dictionary<State, Action> _generationSteps;
        private readonly IntVector2 _gridPositionMin;
        private readonly UnityRandom _random;
        private readonly World _world;
        private readonly Vector2 _worldPositionMin;


        private Biome[,] _biomeInformation;
        private BlockType[,] _chunkData; // the blocks to be generated
        private bool[,] _chunkLockData; // true = block at pos(x,y) is locked and cant be overwritten until generation

        public GeneratorChunk(GeneratorChunkLoader chunkLoader, World world, IntVector2 chunkIndex)
        {
            ChunkIndex = chunkIndex;
            _generationSteps = new Dictionary<State, Action>
            {
                {State.Initialized, () => { }},
                {State.BiomePoints, GenerateBiomePoints},
                {State.ClusterPoints, GenerateClusterPoints},
                {State.Filled, FillBlocks},
                {State.CellularAutomaton, RunCellularAutomaton},
                {State.AgentsCenter, GenerateAgentsCenter},
                {State.AgentsUp, GenerateAgentsUp},
                {State.AgentsUpRight, GenerateAgentsUpRight},
                {State.AgentsRight, GenerateAgentsRight},
                {State.AgentsLeft, GenerateAgentsLeft},
                {State.ReadyToTriggerNeighbours, () => { }},
                {State.Done, () => { }},
            };
            _chunkLoader = chunkLoader;
            _world = world;
            _chunkSize = world.ChunkSize;
            _gridPositionMin = _chunkSize*chunkIndex;
            _worldPositionMin = _gridPositionMin;
            _random = new UnityRandom(GetLocalSeed(chunkIndex, world.Seed));
        }


        public void GenerateToState(State targetState)
        {
            while (CurrentState < targetState)
            {
                CurrentState++;
                if (_generationSteps.ContainsKey(CurrentState))
                {
                    _generationSteps[CurrentState]();
                }
            }
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
                    neighbourPos.x >= _chunkSize ||
                    neighbourPos.y >= _chunkSize)
                    continue;

                ret.Add(_chunkData[neighbourPos.x, neighbourPos.y]);
                if (level > 1)
                    ret.AddRange(GetNeighboursOfLevel(level - 1, curPos, direction, true));
            }

            return ret;
        }

        public BlockType GetWorldBlockIndex(IntVector2 worldPosition)
        {
            IntVector2 localIndex = worldPosition - _gridPositionMin;
            return _chunkData[localIndex.x, localIndex.y];
        }

        public BlockType GetChunkBlockIndex(IntVector2 chunkPos)
        {
            return _chunkData[chunkPos.x, chunkPos.y];
        }

        public void SetChunkBlockIndex(IntVector2 chunkPos, BlockType type)
        {
            if (chunkPos.x < 0 || chunkPos.y < 0 ||
                chunkPos.x >= _chunkSize ||
                chunkPos.y >= _chunkSize)
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
            return _gridPositionMin + chunkPos;
        }

        public IntVector2 WorldToChunkPos(IntVector2 worldPos)
        {
            return worldPos - _gridPositionMin;
        }

        public BlockProperties GetBlock(IntVector2 gridPosition)
        {
            IntVector2 localPosition = WorldToChunkPos(gridPosition);
            int x = localPosition.x, y = localPosition.y;
            foreach (Biome.ClusterWeight cluster in _biomeInformation[x, y].Clusters)
            {
                if (_chunkData[x, y] == cluster.Component.Type && cluster.Component.Block != null)
                {
                    return cluster.Component.Block.BlockProperties;
                }
            }
            return BlockProperties.EMPTY;
        }

        public BackgroundProperties GetBackground(IntVector2 gridPosition)
        {
            IntVector2 localPosition = WorldToChunkPos(gridPosition);
            int x = localPosition.x, y = localPosition.y;
            if (_chunkData[x, y] != BlockType.Stone && _chunkData[x, y] != BlockType.Sand &&
                _chunkData[x, y] != BlockType.Dirt && _chunkData[x, y] != BlockType.Cave)
            {
                return BackgroundProperties.EMPTY;
            }
            return _biomeInformation[x, y].Background.BackgroundProperties;
        }

        private void GenerateAgentsLeft()
        {
            _chunkLoader.GetChunk(this, Direction.Left).GenerateToState(State.AgentsRight);
        }


        private void RunCellularAutomaton()
        {
            IEnumerable<Func<List<BlockType>, BlockType>> cellularFuncs = new List<Func<List<BlockType>, BlockType>>
            {
                RandomValue,
                RandomValue,
                MostAppearingValue,
                MostAppearingValue,
            };
            ;
            foreach (Func<List<BlockType>, BlockType> func in cellularFuncs)
            {
                RunCellularStep(func);
            }
        }

        private BlockType RandomValue(List<BlockType> blocks)
        {
            int index = _random.Next(blocks.Count - 1);
            return blocks[index];
        }

        private void GenerateBiomePoints()
        {
            if (_chunkLoader.GetBiomeChunk(this) == this)
            {
                IEnumerable<Vector2> biomePoints = GeneratePoints(_world.AverageBiomePointsCount);
                Biome biome = World.BiomeWeight.GetRandomComponent(_random, _world.Biomes);
                BiomeEntries = biomePoints.Select(biomePoint => new BiomeEntry(biome, biomePoint));
            }
        }

        private IEnumerable<Vector2> GeneratePoints(float averagePointsCount)
        {
            int pointsCount = Mathf.RoundToInt(_random.Possion(averagePointsCount));
            pointsCount = Mathf.Max(pointsCount, 1);
            Vector2[] points = new Vector2[pointsCount];
            for (int i = 0; i < pointsCount; i++)
            {
                points[i] = _worldPositionMin + _chunkSize*new Vector2(_random.NextSingle(), _random.NextSingle());
            }
            return points;
        }


        private void GenerateClusterPoints()
        {
            IEnumerable<BiomeEntry> allBiomes = GetAllBiomePoints();

            int clusterPointsCount = Mathf.RoundToInt((_world.AverageClusterPointsCount));
            IEnumerable<Vector2> clusterPoints = GeneratePoints(clusterPointsCount);
            List<ClusterEntry> clusterEntries = new List<ClusterEntry>();
            foreach (Vector2 clusterPoint in clusterPoints)
            {
                BiomeEntry closestBiome =
                    allBiomes.Aggregate(
                        (minItem, nextItem) =>
                            (clusterPoint - minItem.WorldPosition).sqrMagnitude <
                            (clusterPoint - nextItem.WorldPosition).sqrMagnitude
                                ? minItem
                                : nextItem);
                Biome.Cluster cluster = Biome.ClusterWeight.GetRandomComponent(_random, closestBiome.Biome.Clusters);
                ClusterEntry clusterEntry = new ClusterEntry(closestBiome.Biome, cluster, clusterPoint);
                clusterEntries.Add(clusterEntry);
            }
            ClusterEntries = clusterEntries;
        }


        private IEnumerable<BiomeEntry> GetAllBiomePoints()
        {
            List<GeneratorChunk> biomeChunks = new List<GeneratorChunk>();
            biomeChunks.Add(_chunkLoader.GetBiomeChunk(this));
            biomeChunks.AddRange(DirectionSupport.VALUES.Select(direction => _chunkLoader.GetBiomeChunk(this, direction)));

            List<BiomeEntry> biomes = new List<BiomeEntry>();
            foreach (GeneratorChunk biomeChunk in biomeChunks)
            {
                biomeChunk.GenerateToState(State.BiomePoints);
                biomes.AddRange(biomeChunk.BiomeEntries);
            }
            return biomes;
        }

        private void GenerateAgentsRight()
        {
            _chunkLoader.GetChunk(this, Direction.Down).GenerateToState(State.AgentsUpRight);
            for (int i = _chunkSize*3/4; i < _chunkSize*5/4; i++)
            {
                for (int j = _chunkSize/4; j < _chunkSize*3/4; j++)
                {
                    RunAgent(i, j);
                }
            }
            //_chunkLoader.GetChunk(this, Direction.Left).GenerateToState(State.AgentsRight);
        }

        private void GenerateAgentsUpRight()
        {
            _chunkLoader.GetChunk(this, Direction.Right).GenerateToState(State.AgentsUp);
            for (int i = _chunkSize*3/4; i < _chunkSize*5/4; i++)
            {
                for (int j = _chunkSize*3/4; j < _chunkSize*5/4; j++)
                {
                    RunAgent(i, j);
                }
            }
//            _chunkLoader.GetChunk(this, Direction.Down).GenerateToState(State.AgentsUpRight);
//            _chunkLoader.GetChunk(this, Direction.DownLeft).GenerateToState(State.AgentsUpRight);
//            _chunkLoader.GetChunk(this, Direction.Left).GenerateToState(State.AgentsUpRight);
        }


        private void GenerateAgentsUp()
        {
            _chunkLoader.GetChunk(this, Direction.Up).GenerateToState(State.AgentsCenter);
            for (int i = _chunkSize/4; i < _chunkSize*3/4; i++)
            {
                for (int j = _chunkSize*3/4; j < _chunkSize*5/4; j++)
                {
                    RunAgent(i, j);
                }
            }
            // _chunkLoader.GetChunk(this, Direction.Down).GenerateToState(State.AgentsUp);
        }

        private void GenerateAgentsCenter()
        {
            for (int i = _chunkSize/4; i < _chunkSize*3/4; i++)
            {
                for (int j = _chunkSize/4; j < _chunkSize*3/4; j++)
                {
                    RunAgent(i, j);
                }
            }
        }

        private void RunAgent(int x, int y)
        {
            try
            {
                IntVector2 worldPos = ChunkToWorldPos(new IntVector2(x, y));
                GeneratorChunk chunk = _chunkLoader.GetChunk(worldPos);
                IntVector2 chunkPos = chunk.WorldToChunkPos(worldPos);
                Agent agent = Biome.AgentWeight.GetRandomComponent(_random,
                    chunk._biomeInformation[chunkPos.x, chunkPos.y].Agents);

                agent.Run(new AgentManipulator(_chunkLoader, chunkPos, chunk._gridPositionMin, _chunkSize/4),
                    _random);
            }
            catch (BlockIsLockedException e)
            {
            }
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


        private void FillBlocks()
        {
            _chunkData = new BlockType[_chunkSize, _chunkSize];
            _chunkLockData = new bool[_chunkSize, _chunkSize];
            _biomeInformation = new Biome[_chunkSize, _chunkSize];

            IEnumerable<ClusterEntry> clusters = GetAllClusters();

            for (int i = 0; i < _chunkData.GetLength(0); i++)
            {
                for (int j = 0; j < _chunkData.GetLength(1); j++)
                {
                    IntVector2 target = _gridPositionMin + new IntVector2(i, j);
                    ClusterEntry closestClusterPoint =
                        clusters.Aggregate(
                            (minItem, nextItem) =>
                                (target - minItem.WorldPosition).sqrMagnitude <
                                (target - nextItem.WorldPosition).sqrMagnitude
                                    ? minItem
                                    : nextItem);
                    _chunkData[i, j] = WorldGeneratorGlobalFunctions.IsAboveGround(target.x, target.y)
                        ? BlockType.Empty
                        : closestClusterPoint.Cluster.Type;
                    _chunkLockData[i, j] = false;
                    _biomeInformation[i, j] = closestClusterPoint.Biome;
                }
            }
        }

        private IEnumerable<ClusterEntry> GetAllClusters()
        {
            List<GeneratorChunk> clusterChunks = new List<GeneratorChunk>();
            clusterChunks.Add(this);
            clusterChunks.AddRange(DirectionSupport.VALUES.Select(direction => _chunkLoader.GetChunk(this, direction)));

            List<ClusterEntry> clusters = new List<ClusterEntry>();
            foreach (GeneratorChunk clusterChunk in clusterChunks)
            {
                clusterChunk.GenerateToState(State.ClusterPoints);
                clusters.AddRange(clusterChunk.ClusterEntries);
            }
            return clusters;
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


        private static int GetLocalSeed(IntVector2 chunkIndex, int globalSeed)
        {
            unchecked
            {
                int hashCode = chunkIndex.x;
                hashCode = (hashCode*397) ^ chunkIndex.y;
                hashCode = (hashCode*397) ^ globalSeed;
                return hashCode;
            }
        }

        public class BiomeEntry
        {
            public readonly Biome Biome;
            public readonly Vector2 WorldPosition;

            public BiomeEntry(Biome biome, Vector2 worldPosition)
            {
                Biome = biome;
                WorldPosition = worldPosition;
            }
        }

        public class ClusterEntry
        {
            public readonly Biome Biome;
            public readonly Biome.Cluster Cluster;
            public readonly Vector2 WorldPosition;

            public ClusterEntry(Biome biome, Biome.Cluster cluster, Vector2 worldPosition)
            {
                Biome = biome;
                Cluster = cluster;
                WorldPosition = worldPosition;
            }
        }
    }
}