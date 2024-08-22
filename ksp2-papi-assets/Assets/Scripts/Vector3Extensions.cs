using UnityEngine;

namespace ksp2_papi
{
    public static class Vector3Extensions
    {
        public static Vector4 ToVector4(this Vector3 a, float w) => new Vector4(a.x, a.y, a.z, w);
    }
}