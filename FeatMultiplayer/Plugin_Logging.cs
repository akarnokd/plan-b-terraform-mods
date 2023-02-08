using BepInEx;
using BepInEx.Logging;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace FeatMultiplayer
{
    public partial class Plugin : BaseUnityPlugin
    {
        static ManualLogSource globalLogger;

        static object logExclusion = new object();

        void InitLogging()
        {
            globalLogger = Logger;
        }

        static void Log(int level, object message)
        {
            var md = multiplayerMode;
            if (md == MultiplayerMode.HostLoading || md == MultiplayerMode.Host)
            {
                if (hostLogLevel.Value <= level)
                {
                    AppendLog("Player_Host.log", level, message);
                }
            }
            else if (md == MultiplayerMode.ClientJoin || md == MultiplayerMode.Client)
            {
                if (clientLogLevel.Value <= level)
                {
                    AppendLog("Player_Client_" + clientName + ".log", level, message);
                }
            }
            else
            {
                if (level == 0)
                {
                    globalLogger.LogDebug(message);
                }
                else if (level == 1) {
                    globalLogger.LogInfo(message);
                }
                else if (level == 2)
                {
                    globalLogger.LogWarning(message);
                }
                else if (level == 3)
                {
                    globalLogger.LogError(message);
                }
                else if (level == 4)
                {
                    globalLogger.LogFatal(message);
                }
            }
        }

        static void AppendLog(string logFile, int level, object message)
        {
            lock (logExclusion)
            {
                var path = Path.Combine(Application.persistentDataPath, logFile);

                var sb = new StringBuilder();

                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF"));
                if (level == 0)
                {
                    sb.Append(" | DEBUG   | ");
                }
                else if (level == 1)
                {
                    sb.Append(" | INFO    | ");
                }
                else if (level == 2)
                {
                    sb.Append(" | WARNING | ");
                }
                else if (level == 3)
                {
                    sb.Append(" | ERROR   | ");
                }
                else if (level == 4)
                {
                    sb.Append(" | FATAL   | ");
                }
                sb.Append(message);
                sb.AppendLine();

                File.AppendAllText(path, sb.ToString());
            }
        }

        /// <summary>
        /// Log a debug message to the appropriate log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogDebug(object message)
        {
            Log(0, message);
        }

        /// <summary>
        /// Log an info message to the appropriate log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogInfo(object message)
        {
            Log(1, message);
        }

        /// <summary>
        /// Log a warning message to the appropriate log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(object message)
        {
            Log(2, message);
        }

        /// <summary>
        /// Log an error message to the appropriate log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(object message)
        {
            Log(3, message);
        }

        /// <summary>
        /// Log a fatal message to the appropriate log file.
        /// </summary>
        /// <param name="message"></param>
        public static void LogFatal(object message)
        {
            Log(4, message);
        }

    }
}
