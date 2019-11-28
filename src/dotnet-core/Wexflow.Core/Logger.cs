using System;
using System.Collections.Generic;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Wexflow.Core
{
    /// <summary>
    /// Logger.
    /// </summary>
    public static class Logger
    {
        private static readonly ILog Ilogger = LogManager.GetLogger(typeof(Logger));
        public static List<string> ServerLogs = new List<string>();
        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public static void Info(string msg)
        {
            Ilogger.Info(msg);
            ServerLogs.Add(msg);
        }

        /// <summary>
        /// Logs a formatted information message.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="args">Arguments.</param>
        public static void InfoFormat(string msg, params object[] args)
        {
            Ilogger.InfoFormat(msg, args);
            ServerLogs.Add($"{string.Format(msg, args)}");
        }

        /// <summary>
        /// Logs a Debug log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public static void Debug(string msg)
        {
            Ilogger.Debug(msg);
            ServerLogs.Add(msg);
        }

        /// <summary>
        /// Logs a formatted debug message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public static void DebugFormat(string msg, params object[] args)
        {
            Ilogger.DebugFormat(msg, args);
            ServerLogs.Add($"{string.Format(msg, args)}");
        }

        /// <summary>
        /// Logs an error log message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        public static void Error(string msg)
        {
            Ilogger.Error(msg);
            ServerLogs.Add(msg);
        }

        /// <summary>
        /// Logs a formatted error message.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="args">Arguments.</param>
        public static void ErrorFormat(string msg, params object[] args)
        {
            Ilogger.ErrorFormat(msg, args);
            ServerLogs.Add($"{string.Format(msg, args)}");
        }

        /// <summary>
        /// Logs an error message and an exception.
        /// </summary>
        /// <param name="msg">Log message.</param>
        /// <param name="e">Exception.</param>
        public static void Error(string msg, Exception e)
        {
            Ilogger.Error(msg, e);
            ServerLogs.Add($"{msg} {e.Message}");
        }

        /// <summary>
        /// Logs a formatted log message and an exception.
        /// </summary>
        /// <param name="msg">Formatted log message.</param>
        /// <param name="e">Exception.</param>
        /// <param name="args">Arguments.</param>
        public static void ErrorFormat(string msg, Exception e, params object[] args)
        {
            Ilogger.Error(string.Format(msg, args), e);
            ServerLogs.Add($"{string.Format(msg, args)} {e.Message}");
        }
    }
}
