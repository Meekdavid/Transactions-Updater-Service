using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Models
{
    public class CommunicationModels<T>
    {
        public T? objectValue { get; set; }
        public bool handShakeIsSuccessful { get; set; } = false;
        public List<Log> Logs { get; set; }
    }

    public class ApiResponse
    {
        public string responseCode { get; set; }
        public string responseDescription { get; set; }
        public object nibssResponseCode { get; set; }
        public object sessionId { get; set; }
        public Searchresult[] searchResults { get; set; }
    }

    public class Searchresult
    {
        public string sessionId { get; set; }
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public string destinationBank { get; set; }
        public string beneAccountName { get; set; }
        public string beneAccountNumber { get; set; }
        public decimal amount { get; set; }
        public DateTime transactionDate { get; set; }
        public string statusCode { get; set; }
        public string statusDescription { get; set; }
        public string originatingBank { get; set; }
        public string originatingBankName { get; set; }
        public string destinationBankName { get; set; }
        public string remark { get; set; }
    }


    public class Rootobject
    {
        public string sessionId { get; set; }
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public string destinationBank { get; set; }
        public string beneAccountName { get; set; }
        public string beneAccountNumber { get; set; }
        public float amount { get; set; }
        public DateTime transactionDate { get; set; }
        public string statusCode { get; set; }
        public string statusDescription { get; set; }
        public string originatingBank { get; set; }
        public string originatingBankName { get; set; }
        public string destinationBankName { get; set; }
        public string remark { get; set; }
    }

}
