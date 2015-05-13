#region

using System;
using Assets.Scripts.WorldGenerator;
using Scripts.WorldGenerator;
using UnityEngine;

#endregion

namespace WorldGeneration.Core
{
    public class Biome : MonoBehaviour
    {
        public AgentWeight[] Agents;
        public BackgroundStats Background;
        public ClusterWeight[] Clusters;

        [Serializable]
        public class AgentWeight : WeightedComponent<Agent>
        {
        }

        [Serializable]
        public class Cluster
        {
            public BlockStats Block;
            public BlockType Type;
        }
        [Serializable]
        public class ClusterWeight : WeightedComponent<Cluster>
        {
        }
    }
}