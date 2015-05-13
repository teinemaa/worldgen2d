using System.Collections.Generic;
using System.Linq;
using BlockEngine;
using UnityEngine;

namespace WorldGeneration.Core
{
    public class BiomeBasedWorldGenerator : BlockWorldGenerator
    {

        public World World;

        private GeneratorChunkLoader _chunkLoader;

        // Use this for initialization
        void Start () {
            _chunkLoader = new GeneratorChunkLoader(World);
        }
	
        // Update is called once per frame
        void Update () {
	
        }

        public override BlockProperties GetBlockProperties(int x, int y)
        {
            IntVector2 gridPosition = new IntVector2(x, y);
            GeneratorChunk chunk = _chunkLoader.GetFullyGeneratedChunk(gridPosition);
            return chunk.GetBlock(gridPosition);
        }

        public override BackgroundProperties GetBackgroundProperties(int x, int y)
        {
            IntVector2 gridPosition = new IntVector2(x, y);
            GeneratorChunk chunk = _chunkLoader.GetFullyGeneratedChunk(gridPosition);
            return chunk.GetBackground(gridPosition);
        }

        public override BlockProperties[] GetAllBlockProperies()
        {
            HashSet<BlockProperties> allBlockProperties = new HashSet<BlockProperties>();
            foreach (World.BiomeWeight biome in World.Biomes)
            {
                foreach (Biome.ClusterWeight cluster in biome.Component.Clusters)
                {
                    if (cluster.Component.Block != null)
                    {

                        allBlockProperties.Add(cluster.Component.Block.BlockProperties);
                    }
                }
            }
            return allBlockProperties.ToArray();
        }

        public override BackgroundProperties[] GetAllBackgroundProperies()
        {
            HashSet<BackgroundProperties> backgroundPropertieses = new HashSet<BackgroundProperties>();
            foreach (World.BiomeWeight biome in World.Biomes)
            {
                backgroundPropertieses.Add(biome.Component.Background.BackgroundProperties);
                
            }
            return backgroundPropertieses.ToArray();
        }
    }
}
