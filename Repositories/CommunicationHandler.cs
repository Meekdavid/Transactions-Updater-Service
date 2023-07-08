using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Repositories
{
    public class CommunicationHandler : ICommunicationHandler
    {
        public async Task<CommunicationModels<string>> HttpPostMethod(string destination, string url, ObjectMultiSelect nameVParam)
        {
            string classMethodName = "TransactionsRequest|MPNGetRestClient";
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMethodName}----------------").AppendLine();

            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.AddHeader("content-type", "application/json");
                logBuilder.AppendLine($"The API request directed to MPN for transaction retreival is: {JsonConvert.SerializeObject(nameVParam)}").AppendLine();

                for (int i = 0; i < nameVParam.sItem.Length; i++)
                {
                    request.AddParameter(nameVParam.sItem[i], nameVParam.sValue[i]);
                }

                var restResponse = client.Execute(request);

                //Do not display full object on logs to save space.
                var apiResponseToLog = JsonConvert.DeserializeObject<ApiResponse>(restResponse.Content);
                var logDisplay = new
                {
                    apiResponseToLog.responseCode,
                    apiResponseToLog.responseDescription,
                    apiResponseToLog.nibssResponseCode,
                    apiResponseToLog.sessionId
                };
                logBuilder.AppendLine($"The API response received from MPN is: {JsonConvert.SerializeObject(logDisplay)}").AppendLine();


                if (string.IsNullOrEmpty(restResponse.Content))
                {
                    logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);
                    return new CommunicationModels<string>
                    {
                        objectValue = restResponse.ErrorMessage,
                        handShakeIsSuccessful = false,
                        Logs = logs
                    };
                }

                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                return new CommunicationModels<string>
                {
                    objectValue = restResponse.Content,
                    handShakeIsSuccessful = true,
                    Logs = logs
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_INFORMATION, ref logs, ex, classMethodName + " Exception");
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception occured when converting old account to nuban").AppendLine();
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new CommunicationModels<string>
                {
                    objectValue = ex.Message,
                    handShakeIsSuccessful = false,
                    Logs = logs
                };
            }
            
        }
    }
}
