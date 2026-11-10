using UnityEngine;

namespace GP
{
    public static class AngleUtils
    {

        /// <summary>
        /// 限制角度范围在0-360度之间循环
        /// </summary>
        /// <param name="angle">角度值</param>
        public static float NormalizeAngle360(float angle)
        {
            angle %= 360f; // C# 余数与被除数同号；负角度需要再加 360

            if (angle < 0f) // 保险，当浮点精度不准确会有用
            {
                angle += 360f;
            }

            return angle;
        }

        /// <summary>
        /// 将角度转换为180度范围内
        /// </summary>
        /// <param name="angle"></param>
        public static float NormalizeAngle180(float angle)
        {
            return Mathf.Repeat(angle + 180f, 360f) - 180f;
        }

        /// <summary>
        /// 限制角度范围在min-max之间
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static float ClampAngle(float angle, float min, float max)
        {
            // 将角度和上下限统一到 [-180, 180) 后裁剪
            angle = Mathf.Repeat(angle, 360f);
            if (angle > 180f) angle -= 360f;

            min = Mathf.Repeat(min, 360f);
            max = Mathf.Repeat(max, 360f);

            if (min > 180f) min -= 360f;
            if (max > 180f) max -= 360f;

            return Mathf.Clamp(angle, min, max);
        }
    }
}
