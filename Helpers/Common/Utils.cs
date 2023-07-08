using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Common
{
    public class Utils
    {
        public enum LogType
        {
            /// <summary>
            /// Log Message in Debug Level
            /// </summary>
            [Description("Log Message in Debug Level")]
            LOG_DEBUG = 1,
            /// <summary>
            /// Log Message in Information Level
            /// </summary>
            [Description("Log Message in Information Level")]
            LOG_INFORMATION = 2,
            /// <summary>
            /// Log Message in Error Level
            /// </summary>
            [Description("Log Message in Error Level")]
            LOG_ERROR = 3
        }

        //MESSAGE BODIES
        public const string StatusMessage_Success = "All Checks are Fine";
        public const string DefaultAccount = "Error Occured";
        public const string ConnectionTimeout = "Time out occured when trying to reach the database.";
        public const string DefaultCustomer = "Default Customer";
        public const string DefaultErrorMessage = "Failed";
        public const string Logger_MessageHeader = $"--------------CM--------STARTED--------";
        public const string Logger_MessageFooter = $"--------------CM--------ENDED--------";
        public string Logger_MessageBody = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} MS";

        //DATABASE QUERIES
        public const string QueryDebitTrans = "select * from transact where bra_code = :bra_code and cus_num = :cus_num and cur_code = :cur_code and led_code = :led_code and sub_acct_code = :sub_acct_code and deb_cre_ind = 1 and tra_date >= ':queryStartDate' and tra_date <= ':queryEndDate'";
        public const string QueryCreditTrans = "select * from transact where bra_code = :bra_code and cus_num = :cus_num and cur_code = :cur_code and led_code = :led_code and sub_acct_code = :sub_acct_code and deb_cre_ind = 2 and tra_date >= ':queryStartDate' and tra_date <= ':queryEndDate'";
        public const string QueryCreditSecondLeg = "select bra_code, cus_num, cur_code, led_code, sub_acct_code from transact where origt_tra_date = :origt_tra_date and origt_bra_code = :origt_bra_code and origt_tra_seq1 = :origt_tra_seq1 and deb_cre_ind = 1";
        public const string QueryDebitSecondLeg = "select bra_code, cus_num, cur_code, led_code, sub_acct_code from transact where origt_tra_date = :origt_tra_date and origt_bra_code = :origt_bra_code and origt_tra_seq1 = :origt_tra_seq1 and deb_cre_ind = 2";
    }
}
