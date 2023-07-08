using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings
{
    public class CustomEmail
    {
        public string SendSpecificTrans { get; set; }
        public string SpecificTransDate { get; set; }
        public int NumberOfTimeToResend { get; set; }
    }
}
