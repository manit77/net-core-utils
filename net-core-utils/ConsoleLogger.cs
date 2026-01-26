using System;

namespace CoreUtils
{
    public static class ConsoleLogger
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        public static void WriteLog(string message, LogLevel level = LogLevel.Info)
        {
            // Store original color to reset it later
            ConsoleColor originalColor = Console.ForegroundColor;

            switch (level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[INFO]: {message}");
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARN]: {message}");
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR]: {message}");
                    break;
            }

            // Reset to original color
            Console.ForegroundColor = originalColor;
        }

        // Shorthand helpers
        public static void Info(string msg) => WriteLog(msg, LogLevel.Info);
        public static void Warn(string msg) => WriteLog(msg, LogLevel.Warning);
        public static void Error(string msg) => WriteLog(msg, LogLevel.Error);
    }
}