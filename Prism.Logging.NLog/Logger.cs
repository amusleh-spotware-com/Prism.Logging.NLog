using System;
using System.Diagnostics;
using System.Text;
using NL = NLog;

namespace Prism.Logging.NLog
{
    public class Logger : ILoggerFacade
    {
        #region Constructor

        public Logger()
        {
        }

        public Logger(string internalLogFilePath)
        {
            NL.Common.InternalLogger.LogFile = System.IO.Path.Combine(internalLogFilePath);
        }

        #endregion Constructor

        #region Delegates

        public delegate void OnExceptionHandler(Exception exception, string exceptionText);

        public delegate void OnErrorHandler(string errorMessage);

        #endregion Delegates

        #region Events

        public event OnExceptionHandler OnException;

        public event OnErrorHandler OnError;

        #endregion Events

        #region Properties

        public NL.Logger NLogger { get; } = NL.LogManager.GetCurrentClassLogger();

        #endregion Properties

        #region Methods

        public void Log(string message, Category category, Priority priority)
        {
            switch (category)
            {
                case Category.Debug:
                    NLogger.Trace(message);
                    break;

                case Category.Info:
                    NLogger.Info(message);
                    break;

                case Category.Exception:
                    NLogger.Error(message);
                    break;

                case Category.Warn:
                    NLogger.Warn(message);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        public void LogException(Exception exception, Func<Exception, string> exceptionToTextConverter = null, bool invokeOnException = true)
        {
            string exceptionText = exceptionToTextConverter != null ? exceptionToTextConverter(exception) : GetExceptionText(exception);

            NLogger.Fatal(exceptionText);

            if (invokeOnException)
            {
                OnException?.Invoke(exception, exceptionText);
            }
        }

        public void LogError(string errorMessage, bool invokeOnError = true)
        {
            NLogger.Error(errorMessage);

            if (invokeOnError)
            {
                OnError?.Invoke(errorMessage);
            }
        }

        public void Trace(string message) => NLogger.Trace(message);

        public void Debug(string message) => NLogger.Debug(message);

        public void Warn(string message) => NLogger.Warn(message);

        public void Info(string message) => NLogger.Info(message);

        private string GetExceptionText(Exception exception, bool innerException = false, string intend = null)
        {
            StringBuilder stringBuilder = new StringBuilder();

            intend = intend ?? string.Empty;

            if (innerException)
            {
                stringBuilder.AppendLine();
                stringBuilder.Append($"{intend}InnerException:");
            }
            else
            {
                string systemType = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

                stringBuilder.AppendLine();
                stringBuilder.Append($"OS Version: {Environment.OSVersion.VersionString}");
                stringBuilder.AppendLine();
                stringBuilder.Append($"System Type: {systemType}");
            }

            stringBuilder.AppendLine();
            stringBuilder.Append($"{intend}Source: {exception.Source}");
            stringBuilder.AppendLine();
            stringBuilder.Append($"{intend}Message: {exception.Message}");
            stringBuilder.AppendLine();
            stringBuilder.Append($"{intend}TargetSite: {exception.TargetSite}");
            stringBuilder.AppendLine();
            stringBuilder.Append($"{intend}Type: {exception.GetType()}");
            stringBuilder.AppendLine();

            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                StackTrace exceptionStackTrace = new StackTrace(exception, true);

                StackFrame[] frames = exceptionStackTrace.GetFrames();

                if (frames != null)
                {
                    stringBuilder.Append($"{intend}StackTrace:");
                    stringBuilder.AppendLine();

                    foreach (StackFrame stackFram in frames)
                    {
                        string newIntend = new string(' ', intend.Length + 4);

                        string fileName = stackFram.GetFileName();

                        fileName = !string.IsNullOrEmpty(fileName)
                            ? fileName.Substring(fileName.LastIndexOf(@"\", StringComparison.InvariantCultureIgnoreCase) + 1)
                            : string.Empty;

                        stringBuilder.Append(
                            $"{newIntend}File: {fileName} | Line: {stackFram.GetFileLineNumber()} | Col: {stackFram.GetFileColumnNumber()} | Offset: {stackFram.GetILOffset()} | Method: {stackFram.GetMethod()}");

                        stringBuilder.AppendLine();
                    }
                }
            }

            if (exception.InnerException != null)
            {
                stringBuilder.Append(GetExceptionText(exception.InnerException, innerException: true,
                    intend: new string(' ', intend.Length + 4)));
            }
            else
            {
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        #endregion Methods
    }
}