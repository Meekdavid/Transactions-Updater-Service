using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Models
{
    public class TransReturnObject
    {
        public List<Log> Logs { get; set; }
        public List<CSVDetails> ExcelEntries { get; set; }
    }
}
