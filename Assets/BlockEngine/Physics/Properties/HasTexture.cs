using System;
using UnityEngine;

namespace BlockEngine
{
    public interface HasTexture
    {
        Texture2D GetTexture();

    }

    internal static class HasTextureSupport
    {
        internal static bool HasTexture(this HasTexture hasTexture)
        {
            return hasTexture.GetTexture() != null;
        }
    }

}
