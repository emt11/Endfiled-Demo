using UnityEngine;

namespace Polyart
{
    struct CullFlags
    {
        public const byte CULLED = 0;
        public const byte VISIBLE = 1;
        public const byte PARTIALLY_VISIBLE = 2;
    }

    public static class CullingUtililies
    {
        public static byte TestFrustumAABB(ref Plane plane0, ref Plane plane1, ref Plane plane2, ref Plane plane3, ref Plane plane4, ref Plane plane5, ref Bounds b)
        {
            Vector3 p0 = b.min;
            Vector3 p1 = b.min + Vector3.right * b.size.x;
            Vector3 p2 = b.min + Vector3.forward * b.size.z;
            Vector3 p3 = b.min + Vector3.right * b.size.x + Vector3.forward * b.size.z;
            Vector3 p4 = p0 + Vector3.up * b.size.y;
            Vector3 p5 = p1 + Vector3.up * b.size.y;
            Vector3 p6 = p2 + Vector3.up * b.size.y;
            Vector3 p7 = p3 + Vector3.up * b.size.y;

            if (IsBehindPlaneAABB(ref plane0, ref p0, ref p1, ref p2, ref p3, ref p4, ref p5, ref p6, ref p7)) return CullFlags.CULLED;
            if (IsBehindPlaneAABB(ref plane1, ref p0, ref p1, ref p2, ref p3, ref p4, ref p5, ref p6, ref p7)) return CullFlags.CULLED;
            if (IsBehindPlaneAABB(ref plane2, ref p0, ref p1, ref p2, ref p3, ref p4, ref p5, ref p6, ref p7)) return CullFlags.CULLED;
            if (IsBehindPlaneAABB(ref plane3, ref p0, ref p1, ref p2, ref p3, ref p4, ref p5, ref p6, ref p7)) return CullFlags.CULLED;
            if (IsBehindPlaneAABB(ref plane4, ref p0, ref p1, ref p2, ref p3, ref p4, ref p5, ref p6, ref p7)) return CullFlags.CULLED;
            if (IsBehindPlaneAABB(ref plane5, ref p0, ref p1, ref p2, ref p3, ref p4, ref p5, ref p6, ref p7)) return CullFlags.CULLED;

            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p0)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p1)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p2)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p3)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p4)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p5)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p6)) return CullFlags.PARTIALLY_VISIBLE;
            if (!IsPointInsideFrustum(ref plane0, ref plane1, ref plane2, ref plane3, ref plane4, ref plane5, ref p7)) return CullFlags.PARTIALLY_VISIBLE;

            return CullFlags.VISIBLE;
        }

        public static bool IsBehindPlaneAABB(ref Plane plane, ref Vector3 p0, ref Vector3 p1, ref Vector3 p2, ref Vector3 p3,
            ref Vector3 p4, ref Vector3 p5, ref Vector3 p6, ref Vector3 p7)
        {
            if (plane.GetSide(p0) == true) return false;
            if (plane.GetSide(p1) == true) return false;
            if (plane.GetSide(p2) == true) return false;
            if (plane.GetSide(p3) == true) return false;
            if (plane.GetSide(p4) == true) return false;
            if (plane.GetSide(p5) == true) return false;
            if (plane.GetSide(p6) == true) return false;
            if (plane.GetSide(p7) == true) return false;

            return true;
        }

        public static bool IsPointInsideFrustum(ref Plane plane0, ref Plane plane1, ref Plane plane2, ref Plane plane3, ref Plane plane4, ref Plane plane5, ref Vector3 point)
        {
            if (!plane0.GetSide(point))
                return false;
            if (!plane1.GetSide(point))
                return false;
            if (!plane2.GetSide(point))
                return false;
            if (!plane3.GetSide(point))
                return false;
            if (!plane4.GetSide(point))
                return false;
            if (!plane5.GetSide(point))
                return false;

            return true;
        }
    }
}
