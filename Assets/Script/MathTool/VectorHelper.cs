using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MathHelper.XMathFunc;

namespace MathHelper
{
    public static class Vec
    {
        public static Vector2 xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 yx(this Color v)
        {
            return new Vector2(v.g, v.r);
        }

        public static Vector3 Divide(in Vector3 v1, in Vector3 v2)
        {
            if (NearZero(v2.x) ||
                NearZero(v2.y) ||
                NearZero(v2.z))
            {
                Debug.LogError("Divide 0 vector3!");
            }
            return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }

        public static Vector3 Mul(in Vector3 v1, in Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        public static Vector3Int MulToInt3(in Vector3 v1, in Vector3 v2)
        {
            return new Vector3Int((int)(v1.x * v2.x), (int)(v1.y * v2.y), (int)(v1.z * v2.z));
        }

        public static Vector3 Sub(in Vector3 v, in float k)
        {
            return new Vector3(v.x - k, v.y - k, v.z - k);
        }

        public static Vector3Int ToInt(in Vector3 v)
        {
            return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
        }

        public static Vector2 ToVec2(in float x)
        {
            return new Vector2(x, x);
        }

        public static Vector2 VecXZ(in Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static float GetMax(in Vector3 v)
        {
            return v.x > v.y ? (v.x > v.z ? v.x : v.z) : (v.y > v.z ? v.y : v.z);
        }
    }
}