using System;
using UnityEngine;

namespace BlockEngine
{
    [Serializable]
    public sealed class BackgroundProperties : HasTexture
    {
        public static BackgroundProperties EMPTY = CreateEmpty();

        private static BackgroundProperties CreateEmpty()
        {
            BackgroundProperties toReturn = new BackgroundProperties();
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
