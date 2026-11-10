using UnityEngine;

namespace GP
{
    public static class GizmosUtils
    {
        public static void DrawSingleCylinder(float radius, float height, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // 应用旋转和位置变换
            Gizmos.matrix = Matrix4x4.TRS(position + Vector3.up * height / 2f, Quaternion.Euler(rotation), scale);

            DrawCylinder(radius, height);

            Gizmos.matrix = oldMatrix; // 恢复原始矩阵
        }

        private static void DrawCylinder(float radius, float height)
        {
            Vector3 topCenter = new Vector3(0, height / 2, 0);
            Vector3 bottomCenter = new Vector3(0, -height / 2, 0);

            // 顶部和底部圆环
            DrawCircle(topCenter, radius, Vector3.up);
            DrawCircle(bottomCenter, radius, Vector3.up);

            // 侧面线条
            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI / 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(topCenter + offset, bottomCenter + offset);
            }
        }

        private static void DrawCircle(Vector3 center, float radius, Vector3 axis)
        {
            Vector3 temp = (axis == Vector3.up) ? Vector3.forward : Vector3.up;
            Vector3 right = Vector3.Cross(axis, temp).normalized;
            Vector3 forward = Vector3.Cross(right, axis).normalized;

            const int Segments = 32;

            for (int i = 0; i <= Segments; i++)
            {
                float angle = (2f * Mathf.PI * i) / Segments;
                Vector3 point1 = center + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;

                if (i < Segments)
                {
                    float nextAngle = (2f * Mathf.PI * (i + 1)) / Segments;
                    Vector3 point2 = center + (right * Mathf.Cos(nextAngle) + forward * Mathf.Sin(nextAngle)) * radius;
                    Gizmos.DrawLine(point1, point2);
                }
            }
        }

    }
}
