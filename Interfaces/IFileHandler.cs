using PTAUpdater.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Interfaces
{
    public interface IFileHandler
    {
        public Task<DatabaseResult<bool>> WriteToCSV(string destination, List<CSVDetails> excelEntries, string fileName);
        public Task<DatabaseResult<bool>> FormatCSV(string destination, string fileName, List<CSVDetails> excelentries);
        public Task<DatabaseResult<string>> GetCSVColumnName(string destination, int columnNumber);
    }
}
