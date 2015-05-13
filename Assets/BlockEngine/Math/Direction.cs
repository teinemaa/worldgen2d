using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using System.Collections;

namespace BlockEngine
{
    public enum Direction
    {
        Up = 0,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    };

    internal static class DirectionSupport
    {

        private static readonly IntVector2[] DIRECTION_OFFSETS = new IntVector2[8] { IntVector2.UP, IntVector2.ONE, IntVector2.RIGHT, new IntVector2(1, -1), new IntVector2(0, -1), new IntVector2(-1, -1), new IntVector2(-1, 0), new IntVector2(-1, 1) };
        private static readonly IntVector2[][] BLOCK_OFFSETS = new IntVector2[4][] { new IntVector2[2] { new IntVector2(-1, 0), new IntVector2(0, 0) }, new IntVector2[1] { new IntVector2(0, 0) }, new IntVector2[2] { new IntVector2(0, 0), new IntVector2(0, -1) }, new IntVector2[1] { new IntVector2(0, -1) } };
        private static readonly float[] LENGTHS = new float[8] { 1, Mathf.Sqrt(2), 1, Mathf.Sqrt(2), 1, Mathf.Sqrt(2), 1, Mathf.Sqrt(2) };
        internal static readonly Direction[] VALUES = (Direction[])Enum.GetValues(typeof(Direction));
        internal static readonly Direction[] PRIMARY_DIRECTIONS = new Direction[4]{Direction.Up, Direction.Right, Direction.Down, Direction.Left};
        internal static Direction Opposite(this Direction direction)
        {
            return (Direction)(((int)direction + 4) % 8);
        }
        internal static int Index(this Direction direction)
        {
            return (int)direction;
        }

        internal static IntVector2 Offset(this Direction direction)
        {
            return DIRECTION_OFFSETS[direction.Index()];
        }


        internal static IntVector2[] BlockOffset(this Direction direction)
        {
            return BLOCK_OFFSETS[direction.Index()];
        }

        internal static float Length(this Direction direction)
        {
            return LENGTHS[direction.Index()];
        }

        internal static bool IsBase(this Direction direction)
        {
            return (int)direction / 4 == 0;
        }

        internal static bool IsDiagonal(this Direction direction)
        {
            return (int)direction % 2 == 1;
        }


        internal static Direction GetDirection(Vector2 v)
        {

            float ratio = Mathf.Abs(v.x / v.y);
            bool diagonal = ratio > 0.5f && ratio < 2f;
            if (diagonal)
            {
                if (v.x > 0)
                {
                    return v.y > 0 ? Direction.UpRight : Direction.DownRight;
                }
                else
                {
                    return v.y > 0 ? Direction.UpLeft : Direction.DownLeft;
                }
            }
            else
            {
                if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
                {
                    return v.x > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    return v.y > 0 ? Direction.Up : Direction.Down;
                }
            }
        }
    }

}