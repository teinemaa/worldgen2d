
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.WorldGenerator;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;
using WorldGeneration.Core;
using Random = UnityEngine.Random;

public class MyWorldGenerator : BlockWorldGenerator
{
    public World World;
    public int seed;
    public BlockProperties[] blockProperties;
    public BackgroundProperties[] backgroundProperties;
    public Dictionary<IntVector2, BlockProperties> map = new Dictionary<IntVector2, BlockProperties>();

    private Dictionary<IntVector2, WorldGeneratorChunk> _chunks = new Dictionary<IntVector2, WorldGeneratorChunk>(); 

    private BlockProperties[] _primaryFeatures;
    private WorldGeneratorGlobalFunctions _globalFunctions;

    private void Awake()
    {
        _globalFunctions = new WorldGeneratorGlobalFunctions();
        _primaryFeatures = new BlockProperties[3] { blockProperties[2], blockProperties[1], BlockProperties.EMPTY };
    }

    void FixedUpdate()
    {
        //backgroundProperties[0].strengthMultiplier = (1+ Mathf.Sin(0.5f*Time.time))/2;
        //Physics2D.gravity = -9.81f * new Vector2(Mathf.Sin(Time.time), Mathf.Cos(Time.time)) - 0.5f *9.81f* Vector2.up;
    }

    public override BlockProperties GetBlockProperties(int x, int y)
    {
        IntVector2 blockPosition = new IntVector2(x, y);
        WorldGeneratorChunk chunk = GetGeneratedJunk(blockPosition);
        BlockType blockIndex = chunk.GetWorldBlockIndex(blockPosition);
        return blockIndex < 0 ? BlockProperties.EMPTY : blockProperties[(int) blockIndex];
    }


    private WorldGeneratorChunk GetJunkByIndex(IntVector2 junkIndex)
    {
        if (!_chunks.ContainsKey(junkIndex))
        {
            _chunks[junkIndex] = new WorldGeneratorChunk(_globalFunctions, seed, junkIndex);
        }
        return _chunks[junkIndex];
    }

    private WorldGeneratorChunk GetGeneratedJunk(IntVector2 blockPosition)
    {
        
        int xIndex = WorldPositionToJunkIndex(WorldGeneratorChunk.chunkSize.x, blockPosition.x);
        int yIndex = WorldPositionToJunkIndex(WorldGeneratorChunk.chunkSize.y, blockPosition.y);
        IntVector2 junkIndex = new IntVector2(xIndex, yIndex);
        WorldGeneratorChunk chunk = GetJunkByIndex(junkIndex);
        if (!chunk.Generated)
        {
            List<VoronoiPoint> voronoiPoints = new List<VoronoiPoint>();
            foreach (Direction direction in DirectionSupport.VALUES)
            {
                voronoiPoints.AddRange(GetJunkByIndex(junkIndex + direction.Offset()).VoronoiPoints);
            }
            chunk.FillGround(voronoiPoints, World.Biomes.Select(entry => entry.Component).ToArray());
        }
        return chunk;
    }

    private int WorldPositionToJunkIndex(int junkSize, int position)
    {
        if (position >= 0)
        {
            return position/junkSize;
        }
        else
        {
            return (position+1)/junkSize-1;
        }
    }


    public override BackgroundProperties GetBackgroundProperties(int x, int y)
    {
        IntVector2 blockPosition = new IntVector2(x, y);
        WorldGeneratorChunk chunk = GetGeneratedJunk(blockPosition);
        BlockType blockIndex = chunk.GetWorldBlockIndex(blockPosition);
        return blockIndex == BlockType.Empty ||
            blockIndex == BlockType.Leaves ||
            blockIndex == BlockType.Wood
                ? BackgroundProperties.EMPTY : backgroundProperties[0];
    }

    public override BlockProperties[] GetAllBlockProperies()
    {
        return blockProperties;
    }

    public override BackgroundProperties[] GetAllBackgroundProperies()
    {
        return backgroundProperties;
    }
}
