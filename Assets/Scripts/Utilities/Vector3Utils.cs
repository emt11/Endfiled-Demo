using UnityEngine;

namespace GP
{
    public static class Vector3Utils
    {
        /// <summary>
        /// 获取线段上距离给定点最近的位置
        /// </summary>
        /// <param name="point"></param>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public static Vector3 GetClosestPointOnSegment(this Vector3 point, Vector3 startPoint, Vector3 endPoint)
        {
            var direction = endPoint - startPoint;

            var squaredLength = direction.sqrMagnitude;
            if (squaredLength <= 0f) return startPoint;

            var progress = Mathf.Clamp01(Vector3.Dot(point - startPoint, direction) / squaredLength);
            return startPoint + direction * progress;

        }

        /// <summary>
        /// 根据角色变换计算身体部位的位置
        /// </summary>
        /// <param name="character">Root角色</param>
        /// <param name="partPosition">身体部位的位置</param>
        public static Vector3 GetCharacterPartPosition(this Transform character, Vector3 partPosition)
        {
            var localPoint = character.InverseTransformPoint(partPosition);

            return character.position + character.rotation * localPoint;

            /*
             * 以上写法也可以这样写:
             * Vector3 localPoint = character.InverseTransformPoint(partPosition);
             * return character.TransformPoint(localPoint);
             */

        }

        /// <summary>
        /// 将Vector3转换为XZ平面的向量
        /// </summary>
        /// <param name="vector3"></param>
        public static Vector3 ToXZ(this Vector3 vector3)
        {
            return new Vector3(vector3.x, 0f, vector3.z);
        }

    }
}
