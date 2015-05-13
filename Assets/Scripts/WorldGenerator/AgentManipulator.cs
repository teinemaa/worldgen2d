#region

using System.Collections.Generic;
using BlockEngine;
using Scripts.WorldGenerator;
using UnityEngine;
using WorldGeneration.Core;

#endregion

namespace Assets.Scripts.WorldGenerator
{
    /**
     * Chunk information that is given to an agent. Agents start at a local zero 
     * and write all their changes to a local dictionary chunkChanges,
     * before they write it to the chunk using Finish().
     */

    public class AgentManipulator
    {
        public IntVector2 LocalZero
        {
            get { return _localZero; }
        }

        private readonly int _agentRadius;
        private readonly GeneratorChunkLoader _chunkLoader;
        private readonly IntVector2 _localZero;
        private readonly IntVector2 _chunkZero;

        private readonly Dictionary<IntVector2, BlockType> chunkChanges;
        private readonly Dictionary<IntVector2, bool> chunkLocks;

        public AgentManipulator(GeneratorChunkLoader chunkLoader, IntVector2 localZero, IntVector2 chunkZero, int agentRadius)
        {
            _chunkLoader = chunkLoader;
            _localZero = localZero;
            _chunkZero = chunkZero;
            _agentRadius = agentRadius;

            chunkChanges = new Dictionary<IntVector2, BlockType>();
            chunkLocks = new Dictionary<IntVector2, bool>();
        }

        /**
         * temporarily writes to the chunk changes at localPos until Finish() or Revert() is called.
         * doesLock: locks the tile permanently for rewrites, next write access throws exception
         * ignoreLock: doesnt throw an exception if accessing a locked tile, but doesnt write to it either
         */


        public void SetBlockType(IntVector2 localPos, BlockType type, bool doesLock = false, bool ignoreLock = false)
        {
            if (localPos.Radius > _agentRadius)
            {
                Debug.Log("yolo");
                return;
            }

            if (
                IsLocked(localPos))
            {
                if (ignoreLock)
                    return;
                throw new BlockIsLockedException();
            }

            if (chunkChanges.ContainsKey(localPos))
                chunkChanges[localPos] = type;
            else
                chunkChanges.Add(localPos, type);

            if (chunkLocks.ContainsKey(localPos))
                chunkLocks[localPos] = doesLock;
            else
                chunkLocks.Add(localPos, doesLock);
        }

        public bool IsLocked(IntVector2 localPos)
        {
            if (localPos.Radius > _agentRadius)
            {
                Debug.Log("yolo");
                return true;
            }
            IntVector2 chunkPos = LocalToChunkPos(localPos);
            IntVector2 worldPos = LocalToWorldPos(localPos);
            GeneratorChunk chunk = _chunkLoader.GetChunk(worldPos);
            return chunk.GetChunkBlockLock(chunk.WorldToChunkPos(worldPos)) ||
                   (chunkLocks.ContainsKey(localPos) && chunkLocks[localPos]);
        }

        public void Revert()
        {
            chunkChanges.Clear();
            chunkLocks.Clear();
        }

        /**
         * writes the chunk changes to the chunk.
         */

        public void Finish()
        {
            foreach (KeyValuePair<IntVector2, BlockType> change in chunkChanges)
            {
                IntVector2 chunkPos = LocalToChunkPos( change.Key);
                
                IntVector2 worldPos = LocalToWorldPos(change.Key);
                GeneratorChunk chunk = _chunkLoader.GetChunk(worldPos);
                chunk.SetChunkBlockIndex(chunk.WorldToChunkPos(worldPos), change.Value);
                chunk.SetChunkBlockLock(chunk.WorldToChunkPos(worldPos), chunkLocks[change.Key]);
            }
            Revert();
        }

        /**
         * gets the block type on the position on the chunk, not considering chunk changes
         */

        public BlockType GetBlockType(IntVector2 localPos)
        {            
            if (localPos.Radius > _agentRadius)
            {
                Debug.Log("yolo");
                return BlockType.Empty;
            }
            if (chunkChanges.ContainsKey(localPos))
                return chunkChanges[localPos];


            IntVector2 worldPos = LocalToWorldPos(localPos);
            GeneratorChunk chunk = _chunkLoader.GetChunk(worldPos);
            return chunk.GetChunkBlockIndex(chunk.WorldToChunkPos(worldPos));
        }

        public IntVector2 LocalToChunkPos(IntVector2 localPos)
        {

            return LocalZero + localPos;
        }

        public IntVector2 ChunkToLocalPos(IntVector2 chunkPos)
        {
            return chunkPos - LocalZero;
        }

        public IntVector2 LocalToWorldPos(IntVector2 localPos)
        {
            return LocalToChunkPos(localPos) + _chunkZero;
        }

        public IntVector2 WorldToLocalPos(IntVector2 worldPos)
        {
            return ChunkToLocalPos(worldPos - _chunkZero);
        }


    }
}