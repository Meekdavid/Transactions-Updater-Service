using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Processes
{
    public class Updater
    {
        private readonly ITransactionsRetreiver _TransactionsRetreiver;
        private readonly ICommunicationHandler _CommunicationHandler;
        private readonly IFileHandler _FileHandler;
        private readonly IEmailSender _EmailSender;
        public Updater(ICommunicationHandler communicationHandler, IFileHandler fileHandler, IEmailSender emailSender, ITransactionsRetreiver TransactionsRetreiver)
        {
            _CommunicationHandler = communicationHandler;
            _FileHandler = fileHandler;
            _EmailSender = emailSender;
            _TransactionsRetreiver = TransactionsRetreiver;
        }

        public async Task<List<Log>> PTAUpdaterProcess(string destination, DateObject  dates)
        {
            var creditExcelEntries = new List<CSVDetails>();
            var debitExcelEntries = new List<CSVDetails>();
            string classMeth = "PTAUpdaterProcess";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            try
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Retreiving debit and credit transactions for PTA between {dates.queryStartDate} and {dates.queryEndDate}").AppendLine();

                //Retreive All Transactions
                var creditTransactionsBANK = await _TransactionsRetreiver.ProcessBANK(classMeth, dates, "Credit");
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                creditTransactionsBANK.Logs.AddToLogs(ref logs);

                var creditTransactionsMPN = await _TransactionsRetreiver.ProcessMPN(classMeth, dates, "Credit");
                creditTransactionsMPN.Logs.AddToLogs(ref logs);

                var debitTransactionsBANK = await _TransactionsRetreiver.ProcessBANK(classMeth, dates, "Debit");
                debitTransactionsBANK.Logs.AddToLogs(ref logs);

                var debitTransactionsMPN = await _TransactionsRetreiver.ProcessMPN(classMeth, dates, "Debit");
                debitTransactionsMPN.Logs.AddToLogs(ref logs);

                //Add all retreived transactions to their respective Lists
                creditExcelEntries.AddRange(creditTransactionsBANK.ExcelEntries);
                creditExcelEntries.AddRange(creditTransactionsMPN.ExcelEntries);
                creditExcelEntries.Sort((x,y) => string.Compare(x.DepositorName, y.DepositorName));
                debitExcelEntries.AddRange(debitTransactionsBANK.ExcelEntries);
                debitExcelEntries.AddRange(debitTransactionsMPN.ExcelEntries);
                debitExcelEntries.Sort((x, y) => string.Compare(x.DepositorName, y.DepositorName));

                //Prepare the EXCEL Files
                string fileDate = (bool.Parse(ConfigSettings.CustomEmail.SendSpecificTrans)) ? DateTime.Parse(ConfigSettings.CustomEmail.SpecificTransDate).ToString("ddMMMyyyy")
                    : DateTime.Now.AddDays(-1).ToString("ddMMMyyyy");
                string debitFileName = $"{ConfigSettings.WebConfigAttributes.DebitFilePrefix}{fileDate}";
                string creditFileName = $"{ConfigSettings.WebConfigAttributes.CreditFilePrefix}{fileDate}";

                var formatCreditEntriesToExcel = await _FileHandler.FormatCSV(classMeth, creditFileName, creditExcelEntries);
                formatCreditEntriesToExcel.Logs.AddToLogs(ref logs);

                var formatDebitEntriesToExcel = await _FileHandler.FormatCSV(classMeth, debitFileName, debitExcelEntries);
                formatDebitEntriesToExcel.Logs.AddToLogs(ref logs);

                //Send Email to PTA
                var filePaths = new EmailFilePaths
                {
                    creditFileName = creditFileName,
                    debitFileName = debitFileName,
                };

                DateTime emailDate = (bool.Parse(ConfigSettings.CustomEmail.SendSpecificTrans)) ? DateTime.Parse(ConfigSettings.CustomEmail.SpecificTransDate)
                    : DateTime.Now.AddDays(-1);
                string fromattedDate = emailDate.ToString("MMMM yyyy");
                string dayWithOrdinal = $"{emailDate.Day}{StringExtensions.GetDayOrdinal(emailDate.Day)}";
                string formattedEmailDate = $"{dayWithOrdinal} {fromattedDate}";

                var emailSent = await _EmailSender.SendEmailAsync(classMeth, formattedEmailDate, filePaths);
                emailSent.Logs.AddToLogs(ref logs);
                

            }
            catch(Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An error occured in the Updater class.").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                return logs;
            }
            finally
            {
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                Task.Run(() => LogWriter.WriteLog(logs));//Write all saved logs to sink and console.
            }
            return logs;
        }
    }
}
