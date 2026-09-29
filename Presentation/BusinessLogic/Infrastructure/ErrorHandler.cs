using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BusinessLogic.Infrastructure
{
    // Lightweight centralized error/logger for BusinessLogic layer.
    // Writes to a simple file and Debug output. Safe for concurrent use.
    public static class ErrorHandler
    {
        private static readonly object _lock = new object();
        private static readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? ".", "application.log");

        public static void LogInfo(string message)
        {
            Write("INFO", message);
        }

        public static void LogWarning(string message)
        {
            Write("WARN", message);
        }

        public static void LogError(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}";
                Debug.WriteLine(line);

                lock (_lock)
                {
                    File.AppendAllText(_logFile, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // Intentionally swallow logging errors to avoid crash loops.
            }
        }
    }
}
