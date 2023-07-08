using BANKEncryptLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings
{
    public class WebConfigAttributes
    {
        public int jobDelay { get; set; }
        public int ExccelColumnWidth { get; set; }
        public string AllowedIPs { get; set; }
        public string MPNOutward { get; set; }
        public string MPNInward { get; set; }
        public string PTAAccountsNuban { get; set; }
        public string PTAAccountsOld { get; set; }
        public string MPNLedgers { get; set; }
        public string BankName { get; set; }
        public string RawFilesPath { get; set; }
        public string FormattedFilePath { get; set; }
        public string EncryptedFilePath { get; set; }
        public string DefaultAccountNumberWhenErrorOccurs { get; set; }
        public string EmailSubject { get; set; }
        public string MailSender { get; set; }
        public string MailSenderAlias { get; set; }
        public string EmailServer { get; set; }
        public string EmailRecipient { get; set; }
        public string EmailCopy { get; set; }
        public string EmailFooter { get; set; }
        public string EmailPort { get; set; }
        public string CreditFilePrefix { get; set; }
        public string DebitFilePrefix { get; set; }
        public string FilePassword { get; set; }
        public string LockFilesWithPassword { get; set; }

        public string DecryptedFilePassword
        {
            get
            {
                return BANKEncryptLib.DecryptText(FilePassword);
            }
            set
            {
                FilePassword = value;
            }
        }
    }
}
