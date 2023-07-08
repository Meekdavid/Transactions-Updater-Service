﻿using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Extensions
{
    public static class LogExtensions
    {
        public static void AddToLogs(this List<Log> logs, ref List<Log> mainLogs)
        {
            mainLogs.AddRange(logs);
        }
    }
}
