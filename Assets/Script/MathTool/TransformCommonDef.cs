using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MathHelper.XMathFunc;
namespace MathHelper
{
    public static class TransformCommonDef
    {
        public static void GetUVFromNormal(Vector3 n, out Vector3 u, out Vector3 v)
        {
            n = n.normalized;
            Vector3 helper = new Vector3(1, 0, 0);
            if( abs(dot(helper,n))>0.999f)
            {
                helper = new Vector3(0, 1, 0);
            }
            u = cross(n,helper).normalized;
            v = cross(u, n).normalized;
        }

        public static Vector3 EnsureUpAngle(Vector3 dir, float angle)
        {
            dir = dir.normalized;
            if(dir.magnitude == 0)
            {
                Debug.LogError("zero dir");
                return Vector3.forward;
            }
            // 将 dir 向量投影到水平平面上（即 y 轴为零）
            Vector3 dirFlat = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
            // 计算从 dirFlat 指向 dir 的旋转值
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, dirFlat);
            // 将旋转值绕着世界空间的 x 轴旋转目标角度
            rotation = rotation * Quaternion.AngleAxis(-angle, Vector3.right) ;
            // 应用旋转值到 dir 向量上
            dir = rotation * Vector3.forward;
            return dir.normalized;
        }

        public static Vector2 CartesianToSpherical2D(Vector2 xy)
        {
            float r = length(xy);
            xy *= 1.0f / r;
            float theta = Mathf.Atan2(xy.y, xy.x); //Mathf.Atan2 [-PI,PI]
                                            //Debug.Log(phi);
            theta += (theta < 0.0f) ? 2.0f * Mathf.PI : 0.0f; // only if you want [0,2pi)

            return new Vector2(theta, r);
        }

        //x: theta [0,2PI)
        //y: phi [0,PI]
        //z: r
        public static Vector3 CartesianToSpherical(Vector3 xyz)
        {
            float r = length(xyz);
            xyz *= 1.0f / r;
            float phi = acos(xyz.z);

            if (NearZero(xyz.x) && NearZero(xyz.y))
            {
                return new Vector3(0, phi, r);
            }

            float theta = Mathf.Atan2(xyz.y, xyz.x); //atan2 [-PI,PI]
            theta += (theta < 0) ? 2 * Mathf.PI : 0;

            return new Vector3(theta, phi, r);
        }

        public static Vector2 SphericalToCartesian2D(Vector2 sp)
        {
            return new Vector2(sp.y * cos(sp.x), sp.y * sin(sp.x));
        }

        public static Vector3 SphericalToCartesian(float phi, float theta, float r = 1)
        {
            return new Vector3(
                    r * sin(theta) * cos(phi),
                    r * sin(theta) * sin(phi),
                    r * cos(theta)
                );
        }

        public static Vector3 SphericalToCartesian(Vector3 v)
        {
            return SphericalToCartesian(v.x, v.y, v.z);
        }

        public static Vector3 VecXZ(Vector2 v2)
        {
            return new Vector3(v2.x, 0, v2.y);
        }
    }
}