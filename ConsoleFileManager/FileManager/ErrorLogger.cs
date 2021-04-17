using System;
using System.IO;
using ConsoleFileManager.Properties;

namespace ConsoleFileManager.FileManager
{
    /// <summary>
    /// Represents simple logger to log actions and errors in the application.
    /// </summary>
    public static class ErrorLogger
    {
        private static uint currentTrial = 1;
        
        
        /// <summary> Get or set the maximal trials amount for logger to try to log the error happened inside itself. </summary>
        public static uint SelfErrorLoggingMaxTrialsAmount { get; set; } = 5;

        /// <summary> Get or set the extension for error log files. </summary>
        public static string LogFilesExtension { get; set; } = ".txt";

        
        /// <summary>
        /// Get the log file name for specified error.
        /// </summary>
        /// <example>
        /// The file name is "{log type} {current date and time} {type of error}.{extension}":
        /// "Error 13-04-2021 20.52.51 NullReferenceException.txt"
        /// </example>
        /// <param name="error">An error to create log file for.</param>
        /// <returns>Name of the log file. </returns>
        private static string CreateFileName(Exception error)
        {
            const string logType = "Error";
            var errorTime = DateTime.Now.ToString().Replace('.', '-').Replace(':', '.');
            var errorName = error.GetType().Name;

            return $"{logType} {errorTime} {errorName}{LogFilesExtension}";
        }
        
        /// <summary>
        /// Create a file with description of specified error.
        /// </summary>
        /// <param name="error">The error to log.</param>
        public static void LogError(Exception error)
        {
            try
            {
                var errorFileName = CreateFileName(error);
                var errorFilePath = Path.Combine(Settings.Default.errorsLogsDirectory, errorFileName);

                using var fs = new FileStream(errorFilePath, FileMode.Create, FileAccess.Write);
                using var sw = new StreamWriter(fs);

                sw.WriteLine($"Произошла ошибка в {error.Source}: {error.Message}");
                sw.WriteLine("");
                sw.WriteLine(error.StackTrace);
                sw.WriteLine("");
                if (!string.IsNullOrEmpty(error.HelpLink))
                    sw.WriteLine($"Для более подробной информации посетите сайт: {error.HelpLink}");

                currentTrial = 1;
            }
            catch (Exception e)
            {
                // if, suddenly, an exception was thrown while logging other exception
                // then try to log this exception, but not more times than specified
                // (to avoid infinite trying of logging the error)
                if (currentTrial < SelfErrorLoggingMaxTrialsAmount)
                {
                    currentTrial++;
                    LogError(e);
                }
            }
            
        }
    }
}