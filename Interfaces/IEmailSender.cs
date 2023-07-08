using PTAUpdater.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Interfaces
{
    public interface IEmailSender
    {
        public Task<EmailModel<bool>> SendEmailAsync(string destination, string date, EmailFilePaths fileNames);
    }
}
