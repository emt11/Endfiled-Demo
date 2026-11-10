using System;


namespace GP
{
    public static class AnimationTimeUtility
    {

        // chosen because it will cause no rounding errors between time/frames for frames values up to at least 10 million
        private const double TimeEpsilon = 1e-14;
        private const double MaxTimelineDurationInSeconds = 9e6; // 104 days of running time

        /// <summary>
        /// 将一个时间点转换为对应的帧数
        /// </summary>
        /// <remarks>
        /// 如果是动画的总长度和动画的帧率，则得到该动画的实际总帧数
        /// </remarks>
        /// <param name="time">时间点，单位为秒</param>
        /// <param name="frameRate">帧率</param>
        /// <returns>对应的帧数</returns>
        public static int ToFrames(double time, double frameRate)
        {
            ValidateFrameRate(frameRate);
            time = Math.Min(Math.Max(time, -MaxTimelineDurationInSeconds), MaxTimelineDurationInSeconds);
            // this matches OnFrameBoundary
            double tolerance = GetEpsilon(time, frameRate);
            if (time < 0)
            {
                return (int)Math.Ceiling(time * frameRate - tolerance);
            }
            return (int)Math.Floor(time * frameRate + tolerance);
        }

        /// <summary>
        /// 将一个时间点转换为对应的精确帧数
        /// 用于需要精确帧数的场景，例如动画播放时的帧数计算
        /// </summary>
        /// <remarks>
        ///该方法返回的精确帧数，用于需要精确帧数的场景，例如动画播放时的帧数计算
        /// </remarks>
        /// <example>
        /// 用当前流逝的时间 * 帧率，即可得到对应的精确帧数。除以总帧数，就能得出一个非常丝滑的 0.0 到 1.0 之间的进度条数值
        /// </example>
        /// <param name="time">时间点，单位为秒</param>
        /// <param name="frameRate">帧率</param>
        /// <returns>对应的精确帧数</returns>
        public static double ToExactFrames(double time, double frameRate)
        {
            ValidateFrameRate(frameRate);
            return time * frameRate;
        }

        /// <summary>
        /// 将一个帧数转换为对应的时间点
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="frameRate"></param>
        public static double FromFrames(int frames, double frameRate)
        {
            ValidateFrameRate(frameRate);
            return (frames / frameRate);
        }

        /// <summary>
        /// 将一个精确帧数转换为对应的时间点
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="frameRate"></param>
        public static double FromFrames(double frames, double frameRate)
        {
            ValidateFrameRate(frameRate);
            return frames / frameRate;
        }

        public static bool OnFrameBoundary(double time, double frameRate)
        {
            return OnFrameBoundary(time, frameRate, GetEpsilon(time, frameRate));
        }

        public static double GetEpsilon(double time, double frameRate)
        {
            return Math.Max(Math.Abs(time), 1) * frameRate * TimeEpsilon;
        }

        public static int PreviousFrame(double time, double frameRate)
        {
            return Math.Max(0, ToFrames(time, frameRate) - 1);
        }

        public static int NextFrame(double time, double frameRate)
        {
            return ToFrames(time, frameRate) + 1;
        }

        public static double PreviousFrameTime(double time, double frameRate)
        {
            return FromFrames(PreviousFrame(time, frameRate), frameRate);
        }

        public static double NextFrameTime(double time, double frameRate)
        {
            return FromFrames(NextFrame(time, frameRate), frameRate);
        }

        public static bool OnFrameBoundary(double time, double frameRate, double epsilon)
        {
            ValidateFrameRate(frameRate);

            double exact = ToExactFrames(time, frameRate);
            double rounded = Math.Round(exact);

            return Math.Abs(exact - rounded) < epsilon;
        }

        public static double RoundToFrame(double time, double frameRate)
        {
            ValidateFrameRate(frameRate);

            var frameBefore = (int)Math.Floor(time * frameRate) / frameRate;
            var frameAfter = (int)Math.Ceiling(time * frameRate) / frameRate;

            return Math.Abs(time - frameBefore) < Math.Abs(time - frameAfter) ? frameBefore : frameAfter;
        }

        public static string TimeAsFrames(double timeValue, double frameRate, string format = "F2")
        {
            if (OnFrameBoundary(timeValue, frameRate)) // make integral values when on time borders
                return ToFrames(timeValue, frameRate).ToString();
            return ToExactFrames(timeValue, frameRate).ToString(format);
        }

        private static void ValidateFrameRate(double frameRate)
        {
            if (frameRate <= TimeEpsilon || double.IsNaN(frameRate) || double.IsInfinity(frameRate))
                throw new ArgumentException("frame rate cannot be 0 or negative");
        }

    }
}
