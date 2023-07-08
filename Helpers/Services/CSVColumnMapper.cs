using PTAUpdater.Helpers.Models;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Services
{
    public class CSVColumnMapper : ClassMap<CSVDetails>
    {
        public CSVColumnMapper()
        {
            Map(r => r.DepositorName).Name("DEPOSITOR NAME");
            Map(r => r.BankReferenceNo).Name("BANK REFERENCE NO.");
            Map(r => r.TransactionDate).Name("TRANSACTION DATE");
            Map(r => r.Amount).Name("AMOUNT");
            Map(r => r.OriginatorBank).Name("ORIGINATOR BANK");
            Map(r => r.DestinationBank).Name("DESTINATION BANK");
            Map(r => r.DebitAccountName).Name("DEBIT ACCOUNT NAME");
            Map(r => r.CreditAccountName).Name("CREDIT ACCOUNT NAME");
            Map(r => r.DebitAccount).Name("DEBIT ACCOUNT");
            Map(r => r.CreditAccount).Name("CREDIT ACCOUNT");
            Map(r => r.Remark).Name("REMARKS");
            Map(r => r.TransactionType).Name("TRANSACTION MODE");
        }
    }
}
