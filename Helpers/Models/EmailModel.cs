using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PTAUpdater.Helpers.Models
{
    public class EmailModel<T>
    {
        public T? objectValue { get; set; }
        public List<Log> Logs { get; set; }
    }

    public class EmailFilePaths
    {
        public string debitFileName { get; set; }
        public string creditFileName { get; set; }
    }
}
