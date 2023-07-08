using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTAUpdater.Helpers.Models;

namespace PTAUpdater.Interfaces
{
    public interface ISQLBinary
    {
        public Task<DatabaseResult<DataTable>> SqlSelectQueryBasisWithParams(string destination, string ConnString, string CommandQuery, CommandType cmdType, SqlParameter[] param, DateObject dates, object entries, int type);
        public Task<DatabaseResult<string>> ConvertToNuban(string destination, string oldAccountNumber);
        public Task<DatabaseResult<string>> GetCustomerName(string destination, string Nuban);
    }
}
