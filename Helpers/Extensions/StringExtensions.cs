using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTAUpdater.Helpers.Common;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Helpers.Extensions
{
    public static class StringExtensions
    {        
        public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }

        public static void AddToLogs(this string messageLog, ref List<Log> logs, LogType logType = LogType.LOG_DEBUG, Exception exceptionLog = null)
        {
            logs.Add(new Log()
            {
                MessageLog = messageLog,
                LogType = (int)logType,
                ExceptionLog = exceptionLog
            });
        }
        public static string GetDayOrdinal(int day)
        {
            if (day >= 11 && day <= 13)
            {
                return "th";
            }

            switch (day % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }
    }
}
