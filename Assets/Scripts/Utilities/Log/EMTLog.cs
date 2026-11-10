using UnityEngine;

namespace GP
{
    public static class EMTLog
    {
        public static void Log(object message)
        {
            Debug.unityLogger.Log(LogType.Log,
                $"<color=#ffa500ff><b><size=12>[EMT 😘💕🥰]: \n {message}</size></b></color>");
        }

        public static void Warning(object message)
        {
            Debug.unityLogger.Log(LogType.Warning,
                $"<color=#0000ffff><b><size=12>💡[Warning]: \n {message}</size></b></color>");
        }

        public static void Error(object message)
        {
            Debug.unityLogger.Log(LogType.Error,
                $"<color=#ff0000ff><b><size=12>🚨[Error]: \n {message}</size></b></color>");
        }
    }
}
