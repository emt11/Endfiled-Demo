using UnityEngine;

namespace GP
{
    public static class GMathf
    {
        /// <summary>
        /// 判断两个浮点数是否近似相等(两个值的误差小于0.01则表示近似相等)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static bool Approximately(float a, float b)
        {
            return Mathf.Abs(a - b) <= 0.01f;
        }

    }
}
