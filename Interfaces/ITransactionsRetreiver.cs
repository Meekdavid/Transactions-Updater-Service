using PTAUpdater.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Interfaces
{
    public interface ITransactionsRetreiver
    {
        public Task<TransReturnObject> ProcessBANK(string destination, DateObject dates, string Type);
        public Task<TransReturnObject> ProcessMPN(string destination, DateObject dates, string Type);
    }
}
