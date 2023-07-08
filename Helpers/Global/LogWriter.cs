using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Helpers.Global
{
    public static class LogWriter
    {
        private static ILogger _Logger;

        public static ILogger Logger
        {
            set
            {
                _Logger = value;
            }
        }

        /// <summary>
        /// Write Log to the Configured Sink
        /// </summary>
        /// <param name="logs">List of Log Objects to write to Sink.</param>
        /// <returns>Initiated Token Details</returns>
        /// <response code="200">Returns the Initiated Token Details</response>
        public static void WriteLog(List<Log> logs)
        {
            foreach (var log in logs)
            {
                MainLogWriter(log.MessageLog, (LogType)log.LogType, log.ExceptionLog);
            }
        }

        /// <summary>
        /// Write Log to the Configured Sink
        /// </summary>
        /// <param name="messageLog">Message Log as Text</param>
        /// <param name="logType">Logging Type to Use</param>
        /// <param name="exceptionLog">Exception Object if any.</param>
        /// <returns>Initiated Token Details</returns>
        /// <response code="200">Returns the Initiated Token Details</response>
        public static void WriteLog(string messageLog, LogType logType, Exception exceptionLog = null)
        {
            MainLogWriter(messageLog, logType, exceptionLog);
        }

        private static void MainLogWriter(string messageLog, LogType logType, Exception exceptionLog)
        {
            try
            {
                switch (logType)
                {
                    case LogType.LOG_DEBUG:

                        //_Logger.LogDebug(exceptionLog, messageLog);
                        _Logger.LogInformation(messageLog);                        

                        break;
                    case LogType.LOG_INFORMATION:

                        _Logger.LogInformation(messageLog);                        

                        break;
                    case LogType.LOG_ERROR:

                        _Logger.LogError(exceptionLog.ToString());

                        break;
                    default:
                        //DO NOTHING
                        break;
                }
            }
            catch (Exception ex)
            {
                //Log to Windows Event Loggger
                var eventLog = new EventLog();

                if (!EventLog.SourceExists("PTAUpdater"))
                {
                    EventLog.CreateEventSource("PTAUpdater", "Application");
                }

                eventLog.Source = "PTAUpdater";
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Information);
            }
        }       

        
        public static void AddLogAndClearLogBuilderOnException(ref StringBuilder logBuilder, LogType logType, ref List<Log> logs, Exception exception, string exceptionMessage = null)
        {
            logs.Add(new Log()
            {
                LogType = (int)logType,
                MessageLog = logBuilder.ToString()
            });
            logBuilder.Clear();

            logs.Add(new Log()
            {
                LogType = (int)LogType.LOG_ERROR,
                MessageLog = exceptionMessage,
                ExceptionLog = exception
            });
        }
    }
}
