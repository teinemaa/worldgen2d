using System;
using System.Collections.Generic;
using BlockEngine.Objects.Core;
using BlockEngine.Objects.Junks;
using UnityEngine;
using System.Collections;

namespace BlockEngine
{
    public class BlockEngine : MonoBehaviour
    {
        

        public BlockWorldGenerator blockWorldGenerator;
        public Shader blockShader;
        public Texture2D emptyBackgroundTexture;
        public FilterMode blockTextureFilterMode;
        public float globalDampening;
        public float gravityScale;
        private JunkLoader _junkLoader;

        void Start()
        {
            new GameObject("Blocks Renderer", new Type[] { typeof(BlockWorldToMesh) }).transform.parent = this.transform;
            _junkLoader = new JunkLoader(this);
        }

        void Update()
        {

        }

        internal Block GetBlock(IntVector2 gridPosition)
        {
            return _junkLoader.GetBlock(gridPosition);
        }

        internal Corner GetCorner(IntVector2 gridPosition)
        {
            return _junkLoader.GetCorner(gridPosition);
        }

        internal IEnumerable<Corner> GetCorners(Rect cameraWorld)
        {
            return _junkLoader.GetCurrentCorners(cameraWorld, true);
        }


    }

}


