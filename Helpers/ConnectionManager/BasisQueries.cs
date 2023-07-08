using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.ConnectionManager
{
    public static class BasisQueries
    {
        public const string Query_GetAccountNameFromBasis = " select get_name(bra_code, cus_num,0,0,0) Name from map_acct where map_acc_no = :nuban";
        public const string Query_ConvertNubanToOldAccount = " select bra_code ,cus_num ,cur_code,led_code,sub_acct_code from map_acct where map_acc_no = :nuban";
        public const string Query_ConvertOldAccountToNuban = " select nuban from map_acct where bra_code = :bra_code and cus_num = :cus_num and cur_code = :cur_code and led_code= :led_code and sub_acct_code= :sub_acct_code";
        
    }
}
