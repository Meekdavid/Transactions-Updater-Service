using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Common
{
    public static class DapperQueries
    {
        public static string QueryDebitTrans { get; set; } = "select * from transact where bra_code = :bra_code and cus_num = :cus_num and cur_code = :cur_code and led_code = :led_code and sub_acct_code = :sub_acct_code and deb_cre_ind = 1 and tra_date >= :queryStartDate and tra_date <= :queryEndDate";
        public static string QueryCreditTrans { get; set; } = "select * from transact where bra_code = :bra_code and cus_num = :cus_num and cur_code = :cur_code and led_code = :led_code and sub_acct_code = :sub_acct_code and deb_cre_ind = 2 and tra_date >= :queryStartDate and tra_date <= :queryEndDate";
        public static string QueryCreditSecondLeg { get; set; } = "select bra_code, cus_num, cur_code, led_code, sub_acct_code from transact where origt_tra_date = ':origt_tra_date' and origt_bra_code = :origt_bra_code and origt_tra_seq1 = :origt_tra_seq1 and deb_cre_ind = 1";
        public static string QueryDebitSecondLeg { get; set; } = "select bra_code, cus_num, cur_code, led_code, sub_acct_code from transact where origt_tra_date = ':origt_tra_date' and origt_bra_code = :origt_bra_code and origt_tra_seq1 = :origt_tra_seq1 and deb_cre_ind = 2";
    }
}
