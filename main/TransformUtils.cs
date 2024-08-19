using UnityEngine;

namespace ksp2_papi
{
    public static class TransformUtils
    {
        public static string PathTo(this Transform i)
        {
            var remaining = 20;
            var transform = i;
            var o = "";
            do
            {
                if (transform == null)
                    return o;
                o = transform.name + '/' + o;
                transform = transform.parent;
                remaining--;
            }
            while (remaining > 0);
            return "?!?ERROR?!?: Incomplete path: " + o;
        }
    }
}
