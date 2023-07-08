using PTAUpdater.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Services
{
    public static class GenerateOldAccount
    {
        public static OldAccountMembers GetAccountAttributes(string oldAccountNumber)
        {
            char acctsplit = Convert.ToChar("/");
            string[] accountkey = new string[4];
            string bra_code = null;
            string cus_num = null;
            string cur_code = null;
            string led_code = null;
            string sub_acct_code = null;

            accountkey = oldAccountNumber.Trim().Split(acctsplit);
            bra_code = accountkey[0];
            cus_num = accountkey[1];
            cur_code = accountkey[2];
            led_code = accountkey[3];
            sub_acct_code = accountkey[4];

            var attri = new OldAccountMembers();
            attri.cur_code = cur_code;
            attri.bra_code = bra_code;
            attri.cus_num = cus_num;
            attri.led_code = led_code;
            attri.sub_acct_code = sub_acct_code;

            return attri;
        }

        public static OldAccountMembers GetOldAccountSecondLeg(DataTable secondLegTable)
        {
            var returnObject = new OldAccountMembers();

            if (secondLegTable?.Rows.Count > 0)
            {
                DataRow oldAccountObject = secondLegTable.Rows[0];
                returnObject.bra_code = oldAccountObject[0].ToString();
                returnObject.cus_num = oldAccountObject[1].ToString();
                returnObject.cur_code = oldAccountObject[2].ToString();
                returnObject.led_code = oldAccountObject[3].ToString();
                returnObject.sub_acct_code = oldAccountObject[4].ToString();
            }

            return returnObject;
        }
    }
}
