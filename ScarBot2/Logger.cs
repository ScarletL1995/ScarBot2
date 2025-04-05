using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace LoggingLib
{
    public enum LOGLEVEL
    {
        INFO,
        WARN,
        ERROR,
        DEBUG
    }

    public class LOGGER
    {
        private string logFilePath;
        private LOGLEVEL currentLogLevel;
        private long maxLogFileSize;
        private bool consoleLogging;
        private bool jsonLogging;

        private static readonly Dictionary<Type, string> ExceptionMessages = new()
        {
            { typeof(NullReferenceException), "A null reference was encountered." },
            { typeof(IndexOutOfRangeException), "Attempted to access an invalid index." },
            { typeof(ArgumentException), "An invalid argument was provided." },
            { typeof(ArgumentNullException), "A required argument was null." },
            { typeof(ArgumentOutOfRangeException), "An argument was outside the valid range." },
            { typeof(InvalidOperationException), "An invalid operation was attempted." },
            { typeof(FileNotFoundException), "The specified file was not found." },
            { typeof(IOException), "An I/O error occurred." },
            { typeof(UnauthorizedAccessException), "Access to the resource is denied." },
            { typeof(DivideByZeroException), "Attempted to divide by zero." },
            { typeof(FormatException), "Invalid format encountered during parsing." },
            { typeof(OverflowException), "A numeric overflow occurred." },
            { typeof(TimeoutException), "The operation timed out." },
            { typeof(NotImplementedException), "This feature is not yet implemented." },

            { typeof(Win32Exception), "A Windows API error occurred." },
            { typeof(ExternalException), "An external error occurred, possibly from an external system call." },
            { typeof(SocketException), "A network socket error occurred." },
            { typeof(WebException), "An error occurred while accessing the internet." },
            { typeof(UnauthorizedAccessException), "You do not have permission to modify the registry." },
            { typeof(SystemException), "A system process error occurred." },
            { typeof(InvalidOperationException), "Invalid process operation." }
        };

        public LOGGER(string path = "Logs.log", long maxSize = 10 * 1024 * 1024)
        {
            logFilePath = path;
            maxLogFileSize = maxSize;
            currentLogLevel = LOGLEVEL.INFO;
            consoleLogging = true;
            jsonLogging = false;
        }

        public void SetLogFile(string path, long maxSize = 10 * 1024 * 1024)
        {
            logFilePath = path;
            maxLogFileSize = maxSize;
        }

        public void SetLogLevel(LOGLEVEL logLevel)
        {
            currentLogLevel = logLevel;
        }

        public void EnableConsoleLogging(bool enable)
        {
            consoleLogging = enable;
        }

        public void EnableJsonLogging(bool enable)
        {
            jsonLogging = enable;
        }

        public void Log(string message, LOGLEVEL logLevel)
        {
            if (logLevel >= currentLogLevel)
            {
                Console.WriteLine($"{logLevel}: {message}");
                WriteLog(message);
            }
        }

        public void Log(string message, LOGLEVEL logLevel, Exception? exception)
        {
            if (logLevel >= currentLogLevel)
            {
                Console.WriteLine($"{logLevel}: {message}");
                if (exception != null)
                {
                    Console.WriteLine($"Exception: {exception.Message}");
                    Console.WriteLine(exception.StackTrace);
                }
                WriteLog(message);
            }
        }

        private async Task LogAsync(string message, LOGLEVEL level = LOGLEVEL.INFO, Exception? ex = null)
        {
            if (level >= currentLogLevel)
            {
                string callerInfo = GetCallerInfo();
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message} {callerInfo}";

                if (ex != null)
                {
                    string predefinedMessage = ExceptionMessages.ContainsKey(ex.GetType())
                        ? ExceptionMessages[ex.GetType()]
                        : "An unexpected error occurred.";

                    logMessage += $"\n{predefinedMessage}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
                }

                CheckLogFileSize();
                await WriteLogAsync(logMessage);
            }
        }

        private void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(logFilePath, message + Environment.NewLine);

                if (consoleLogging && Environment.UserInteractive)
                {
                    Console.WriteLine(GetLogLevelColor(message) + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private async Task WriteLogAsync(string message)
        {
            try
            {
                await File.AppendAllTextAsync(logFilePath, message + Environment.NewLine);

                if (consoleLogging && Environment.UserInteractive)
                {
                    Console.WriteLine(GetLogLevelColor(message) + message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        private string GetCallerInfo()
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(2);
            var method = frame?.GetMethod();
            var fileName = frame?.GetFileName() ?? "UnknownFile";
            var lineNumber = frame?.GetFileLineNumber() ?? 0;
            return $"[Caller: {method?.DeclaringType?.Name}.{method?.Name}, {fileName}, Line {lineNumber}]";
        }

        private string GetLogLevelColor(string message)
        {
            if (message.Contains("[INFO]")) return "\x1b[32m";
            if (message.Contains("[WARN]")) return "\x1b[33m";
            if (message.Contains("[ERROR]")) return "\x1b[31m";
            if (message.Contains("[DEBUG]")) return "\x1b[34m";
            return "";
        }

        private void CheckLogFileSize()
        {
            FileInfo fileInfo = new FileInfo(logFilePath);
            if (fileInfo.Exists && fileInfo.Length > maxLogFileSize)
            {
                string archiveFile = $"MySQL Logs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
                File.Move(logFilePath, archiveFile);
            }
        }

        public void Log(string message) => Log(message, currentLogLevel);
        public async Task LogAsync(string message) => await LogAsync(message, currentLogLevel);
        public void LogException(Exception ex) => Log("An exception occurred", LOGLEVEL.ERROR, ex);
        public async Task LogExceptionAsync(Exception ex) => await LogAsync("An exception occurred", LOGLEVEL.ERROR, ex);
    }
}
