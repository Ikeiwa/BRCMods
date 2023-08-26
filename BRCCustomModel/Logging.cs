#if !SDK
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRCCustomModel
{
    internal static class Logging
    {
        public static ManualLogSource logger;

        public static void Log(object message)
        {
            if (logger == null) return;
            logger.LogInfo(message);
        }

        public static void LogInfo(object message)
        {
            if (logger == null) return;
            logger.LogInfo(message);
        }

        public static void LogWarning(object message)
        {
            if (logger == null) return;
            logger.LogWarning(message);
        }

        public static void LogError(object message)
        {
            if (logger == null) return;
            logger.LogError(message);
        }

    }
}
#endif