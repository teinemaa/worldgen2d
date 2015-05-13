#region

using System;
using Scripts.WorldGenerator;
using UnityEngine;

#endregion

namespace WorldGeneration.Core
{
    public class World : MonoBehaviour
    {
        [Serializable]
        public class BiomeWeight : WeightedComponent<Biome>
        {
        }

        public BiomeWeight[] Biomes;
        public int ChunkSize
        {
            get { return 40; }
        }

        public int BiomeSizeInChunks
        {
            get { return 3; }
        }

        public float AverageBiomePointsCount
        {
            get
            {
                
                int biomeSize = 2 * BiomeRadius;
                int biomeArea = biomeSize * biomeSize;

                int biomeChunkSize = BiomeSizeInChunks * ChunkSize;
                int biomeChunkArea = biomeChunkSize * biomeChunkSize;

                return   ((float)biomeChunkArea) / biomeArea;

            }
           
        }

        public float AverageClusterPointsCount
        {
            get
            {
                int biomeSize = 2 * ClusterRadius;
                int biomeArea = biomeSize * biomeSize;

                int biomeChunkSize =  ChunkSize;
                int biomeChunkArea = biomeChunkSize * biomeChunkSize;

                return ((float)biomeChunkArea) / biomeArea;
            }
        }

        public int AgentRadius = 10;
        public int BiomeRadius = 100;
        public int ClusterRadius = 1;
        public int Seed = 0;
    }
}