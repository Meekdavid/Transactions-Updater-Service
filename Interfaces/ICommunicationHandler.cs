using PTAUpdater.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Interfaces
{
    public interface ICommunicationHandler
    {
        public Task<CommunicationModels<string>> HttpPostMethod(string destination, string url, ObjectMultiSelect nameVParam);
    }
}
