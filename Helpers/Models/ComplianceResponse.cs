using PTAUpdater.Helpers.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Models
{
    public class ComplianceResponse
    {
        public bool MachineIsAllowed { get; set; } = true;
        public bool InstanceIsSingle { get; set; } = true;
        public string Message { get; set; }
        public List<Log> Logs { get; set; }
    }
}
