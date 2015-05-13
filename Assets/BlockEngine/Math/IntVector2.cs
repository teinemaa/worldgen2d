using System;
using UnityEngine;
using System.Collections;

namespace BlockEngine
{
    public struct IntVector2
    {

        internal static readonly IntVector2 ZERO = new IntVector2(0, 0);
        internal static readonly IntVector2 UP = new IntVector2(0, 1);
        internal static readonly IntVector2 RIGHT = new IntVector2(1, 0);
        internal static readonly IntVector2 ONE = new IntVector2(1, 1);

        public readonly int x;
        public readonly int y;

        public int SqrMagnitude
        {
            get { return x * x + y * y; }
        }

        internal IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int Radius
        {
            get { return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)); }
        }

        public IntVector2 Normalized
        {
            get { return new IntVector2(x > 0 ? 1 : (x < 0 ? -1 : 0), y > 0 ? 1 : (y < 0 ? -1 : 0));  }
        }

        public static IntVector2 operator +(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.x + right.x, left.y + right.y);
        }

        public static IntVector2 operator -(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.x - right.x, left.y - right.y);
        }

        public static IntVector2 operator *(IntVector2 left, IntVector2 right)
        {
            return new IntVector2(left.x * right.x, left.y * right.y);
        }

        public static IntVector2 operator *(IntVector2 left, int right)
        {
            return new IntVector2(left.x * right, left.y * right);
        }

        public static IntVector2 operator *(int left, IntVector2 right)
        {
            return new IntVector2(left * right.x, left * right.y);
        }

        public static bool operator ==(IntVector2 left, IntVector2 right)
        {
            return left.x == right.x && left.y == right.y;
        }

        public static bool operator !=(IntVector2 left, IntVector2 right)
        {
            return left.x != right.x || left.y != right.y;
        }

        public static implicit operator Vector2(IntVector2 value)
        {
            return new Vector2(value.x, value.y);
        }

    }
}