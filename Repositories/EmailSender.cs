using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Helpers.Services;
using PTAUpdater.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Repositories
{
    public class EmailSender : IEmailSender
    {
        public async Task<EmailModel<bool>> SendEmailAsync(string destination, string date, EmailFilePaths fileNames)
        {
            string classMeth = "SendEmailToPTA";
            string debitFilePath, creditFilePath = "";
            SmtpClient sendmail = new SmtpClient();

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            try
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About Sending E-Mail to PTA with the files '{JsonConvert.SerializeObject(fileNames)}' for the date '{date}'").AppendLine();

                var mailMessage = new MailMessage();
                var mailFrom = new MailAddress(ConfigSettings.WebConfigAttributes.MailSender, ConfigSettings.WebConfigAttributes.MailSenderAlias);
                mailMessage.From = mailFrom;

                string[] recipients = ConfigSettings.WebConfigAttributes.EmailRecipient.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (string recipient in recipients)
                {
                    var mailTo = new MailAddress(recipient);
                    mailMessage.To.Add(mailTo);
                }

                string[] emailCopies = ConfigSettings.WebConfigAttributes.EmailCopy.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (emailCopies.Length > 0 && emailCopies[0] != "")
                {
                    foreach (string copy in emailCopies)
                    {
                        var ccopy = new MailAddress(copy);
                        mailMessage.CC.Add(ccopy);
                    }
                }

                mailMessage.Subject = ConfigSettings.WebConfigAttributes.EmailSubject;

                //Implement the Email Body.
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About generating body for the Email").AppendLine();
                string emailBodyTemplate = "./Helpers/Templates/PTAEmailTemplate.html";
                string emailBody = System.IO.File.ReadAllText(emailBodyTemplate);
                emailBody = emailBody.Replace("{Footer}", Salutation.EmailSalutationFooter());
                emailBody = emailBody.Replace("{transdate}", date);

                var altv = AlternateView.CreateAlternateViewFromString(emailBody, null, MediaTypeNames.Text.Html);
                mailMessage.AlternateViews.Clear();
                mailMessage.AlternateViews.Add(altv);
                mailMessage.IsBodyHtml = true;


                //Implement Email Attachments.
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About attaching the excel files to the Email").AppendLine();
                if (bool.Parse(ConfigSettings.WebConfigAttributes.LockFilesWithPassword))
                {
                    debitFilePath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.EncryptedFilePath}/{fileNames.debitFileName}.csv");
                    creditFilePath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.EncryptedFilePath}/{fileNames.creditFileName}.csv");
                }
                else
                {
                    debitFilePath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.FormattedFilePath}/{fileNames.debitFileName}.csv");
                    creditFilePath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.FormattedFilePath}/{fileNames.creditFileName}.csv");
                }

                Attachment attachFile, attachFile2;
                debitFilePath = new Uri(debitFilePath).LocalPath;
                creditFilePath = new Uri(creditFilePath).LocalPath;
                try
                {
                    attachFile = new Attachment(debitFilePath);
                    attachFile2 = new Attachment(creditFilePath);
                    mailMessage.Attachments.Add(attachFile);
                    mailMessage.Attachments.Add(attachFile2);

                }
                catch (Exception ex)
                {
                    attachFile = null;
                    //ON EXCEPTION STORE THE PREVIOUS LOG
                    LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An Error occured while trying to attach files to the Email").AppendLine();
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                }

                sendmail.Host = ConfigSettings.WebConfigAttributes.EmailServer;
                sendmail.Port = Convert.ToInt16(ConfigSettings.WebConfigAttributes.EmailPort);

                sendmail.Send(mailMessage);
                mailMessage.Dispose();
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} E-Mail to PTA with the files '{JsonConvert.SerializeObject(fileNames)}' for the date '{date}' Sucessfully sent.").AppendLine();
            }
            catch(SmtpException SEx)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, SEx, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} SMTP error occured while attempting to send Email").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                return new EmailModel<bool>
                {
                    Logs = logs,
                    objectValue = false
                };
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An error occured while attempting to send Email").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                return new EmailModel<bool>
                {
                    Logs = logs,
                    objectValue = false
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            return new EmailModel<bool>
            {
                Logs = logs,
                objectValue = true
            };
        }
    }
}
