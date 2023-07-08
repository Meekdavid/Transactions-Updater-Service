using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Models
{
    public class CSVDetails
    {
        public string DepositorName { get; set; }
        public string BankReferenceNo { get; set; }
        public string TransactionDate { get; set; }
        public string Amount { get; set; }
        public string OriginatorBank { get; set; }
        public string DestinationBank { get; set; }
        public string DebitAccountName { get; set; }
        public string CreditAccountName { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }        
        public string Remark { get; set; }
        public string TransactionType { get; set; }
        
        //public string DepositorName { get; set; }
    }
}
