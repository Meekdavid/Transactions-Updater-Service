using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Services
{
    public static class FileEncryptor
    {
        public static void EncrypFile(string fileName)
        {
            string password = ConfigSettings.WebConfigAttributes.DecryptedFilePassword;
            string sourcePath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.FormattedFilePath}/{fileName}.csv");
            string location = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.EncryptedFilePath}");

            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }
            string savePath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.EncryptedFilePath}/{fileName}.csv");

            try
            {
                using(var excelPackage = new ExcelPackage(new FileInfo(sourcePath)))
                {
                    excelPackage.Encryption.Password = password;
                    excelPackage.SaveAs(new FileInfo(savePath));
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
