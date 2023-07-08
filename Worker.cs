using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Security.Policy;
using System.Text;
using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Helpers.Services;
using PTAUpdater.Interfaces;
using PTAUpdater.Processes;
using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater
{
    public class Worker : BackgroundService
    {
        //private readonly ILogger<Worker> _logger;
        public static IConfiguration _configs;
        public static ILogger<Worker> _logger;
        private readonly ComplianceCheck _validateThisProcess;
        private string ClassName = string.Empty;
        private readonly ITransactionsRetreiver _Tesring;
        private readonly IFileHandler _dataBaseAgent;
        private readonly Updater _updater;        

        public Worker(ILogger<Worker> logger, IConfiguration configs, ComplianceCheck validateThisProcess,
            ITransactionsRetreiver Tesring, IFileHandler
            databasegent, Updater updater)
        {
            ConfigurationSettingsHelper.Configuration = configs;
            LogWriter.Logger = logger;
            _configs = configs;            
            _validateThisProcess = validateThisProcess;
            ClassName = GetType().Name;
            _Tesring = Tesring;
            _dataBaseAgent = databasegent;
            _updater = updater;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var logBuilder = new StringBuilder(Logger_MessageHeader.Replace("CM", ClassName + ".PTAUpdater Job")).AppendLine();
                
                //Check if machine is allowed to run the job, and if multiple instance is running.
                var preliminaryChecks = _validateThisProcess.ValidateOperation();
                var logs = new List<Log>();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                preliminaryChecks.Logs.AddToLogs(ref logs);

                if (preliminaryChecks.InstanceIsSingle && preliminaryChecks.MachineIsAllowed)
                {
                    //To enable sending transactions for custom dates, else continue with default process.
                    string dateToProcess = ConfigSettings.CustomEmail.SpecificTransDate;
                    if (bool.Parse(ConfigSettings.CustomEmail.SendSpecificTrans))
                    {
                        string[]? dateAndCounter = CachingUtility.GetValueFromCache("value")?.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        
                        if (dateAndCounter != null)
                        {
                            //Caching Mechanism used to control number of times transaction is been sent, when custom is set to TRUE.
                            string dateFromCache = dateAndCounter[0];
                            int counter = int.Parse(dateAndCounter[1]);
                            if (DateTime.Parse(dateFromCache) == DateTime.Parse(dateToProcess) && !(counter > ConfigSettings.CustomEmail.NumberOfTimeToResend))
                            {
                                var datesCustom = new DateObject
                                {
                                    queryStartDate = DateTime.Parse(dateToProcess).Date,
                                    queryEndDate = DateTime.Parse(dateToProcess).Date.AddDays(1),
                                };

                                _updater.PTAUpdaterProcess(ClassName, datesCustom);
                                counter++;
                                string valueToAddToCache = dateToProcess + $",{counter}";
                                CachingUtility.AddValueToCache("value", valueToAddToCache);
                            }
                        }
                        else
                        {
                            var datesCustom = new DateObject
                            {
                                queryStartDate = DateTime.Parse(dateToProcess).Date,
                                queryEndDate = DateTime.Parse(dateToProcess).Date.AddDays(1),
                            };
                            
                            _updater.PTAUpdaterProcess(ClassName, datesCustom);
                            string valueToAddToCache = dateToProcess + ",1";
                            CachingUtility.AddValueToCache("value", valueToAddToCache);
                            Task.Run(() => LogWriter.WriteLog(logs));//Write all saved logs to sink and console.
                        }
                        
                    }

                    //Tasks Should Execute Within 1st 1 Minutes of Every Day.
                    if (DateTime.Now.TimeOfDay.TotalSeconds >= 0 && DateTime.Now.TimeOfDay.TotalSeconds <= 70)
                    {
                        var dates = new DateObject
                        {
                            queryStartDate = DateTime.Now.Date.AddDays(-1),
                            queryEndDate = DateTime.Now.Date,
                        };

                        _updater.PTAUpdaterProcess(ClassName, dates);
                        Task.Run(() => LogWriter.WriteLog(logs));//Write all saved logs to sink and console.
                    }

                }
                else
                {
                    //Shutdown if compliance checks fails
                    Console.WriteLine(preliminaryChecks.Message);
                    await Task.Delay(8000, stoppingToken);
                    Environment.Exit(0);
                }    
                await Task.Delay(ConfigSettings.WebConfigAttributes.jobDelay, stoppingToken);
            }
        }
    }
}