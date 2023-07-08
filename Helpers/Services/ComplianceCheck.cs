using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using static PTAUpdater.Helpers.Common.Utils;
using System.Linq;
using System.Text.RegularExpressions;

namespace PTAUpdater.Helpers.Services
{
    public class ComplianceCheck
    {
        private string ClassName = string.Empty;
        public ComplianceCheck()
        {
            ClassName = GetType().Name;
        }
        public ComplianceResponse ValidateOperation()
        {
            string methodName = "ValidateOperation";
            string classAndMethodName = $"{ClassName}.{methodName}";
            ComplianceResponse checkResponse = new ComplianceResponse();
            checkResponse.Message = StatusMessage_Success;

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About validating machine, and checking for process parrallelism").AppendLine();

            try
            {
                string myHost = Dns.GetHostName();
                var myIPListObject = Dns.GetHostByName(myHost).AddressList;
                string myIP = Dns.GetHostByName(myHost).AddressList[0].ToString();
                var allowedIPs = ConfigSettings.WebConfigAttributes.AllowedIPs.Split(',').ToList();
                List<string> myIPList = new List<string>();

                foreach (var IPs in myIPListObject)
                {
                    myIPList.Add(IPs.ToString());
                }

                if (!allowedIPs.Any(myIPList.Contains))
                {
                    checkResponse.Message = "";
                    checkResponse.Message += $"Machine ({myHost}-{myIP}) isn't Permitted to use this Service";
                    checkResponse.MachineIsAllowed = false;
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} This machine '{myIP}' wasn't permitted to use the service").AppendLine();
                }

                string processName = Process.GetCurrentProcess().ProcessName;
                Process[] currentRunningProcessess = Process.GetProcessesByName(processName);
                int currentProcessCount = currentRunningProcessess.Length;

                if ( currentProcessCount > 1 )
                {
                    checkResponse.Message = (checkResponse.Message == StatusMessage_Success) ? "" : checkResponse.Message + ", Also ";
                    checkResponse.Message += $"Another Intance of {processName} is already Running";
                    checkResponse.InstanceIsSingle = false;
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Another instance of the process is currently running").AppendLine();
                }

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Everything is fine and good, proceeding to next task.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                checkResponse.Logs = logs;
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classAndMethodName + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An Error Occured While Validating Compliancy of the Process.").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                checkResponse.Logs = logs;
            }

            return checkResponse;
        }
    }
}
