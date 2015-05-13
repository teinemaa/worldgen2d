using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldGenerator
{
    public class WorldGeneratorGlobalFunctions
    {

        public static bool IsAboveGround(int x, int y)
        {

                return (y > 7.2f * Mathf.Sin(x / 10f) + 15f * Mathf.Sin(x / 22f));
        }
    }
}
