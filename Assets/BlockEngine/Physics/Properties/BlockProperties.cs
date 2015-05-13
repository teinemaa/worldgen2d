using System;
using UnityEngine;

namespace BlockEngine
{
    [Serializable]
    public sealed class BlockProperties : HasTexture
    {

        internal static readonly BlockProperties EMPTY = CreateEmpty();

        private static BlockProperties CreateEmpty()
        {
            BlockProperties toReturn = new BlockProperties();
            toReturn.texture = null;
            return toReturn;
        }

        public Texture2D texture;

        public Texture2D GetTexture()
        {
            return texture;
        }

    }

}
