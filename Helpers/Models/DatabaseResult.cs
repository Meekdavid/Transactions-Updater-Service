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
    public class DatabaseResult<T>
    {
        public T? objectValue { get; set; }
        public bool queryIsSuccessful { get; set; } = false;
        public List<Log> Logs { get; set; }
    }
}
