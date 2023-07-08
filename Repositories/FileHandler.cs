using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.ConnectionManager;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Helpers.Services;
using PTAUpdater.Interfaces;
using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Repositories
{
    public class FileHandler : IFileHandler
    {
        public async Task<DatabaseResult<bool>> FormatCSV(string destination, string fileName, List<CSVDetails> excelentries)
        {
            string classMeth = "FormatCSV";
            var theReturner = new DatabaseResult<bool>();

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            try
            {               

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var excelPackage = new ExcelPackage())
                {

                    var worksheet = excelPackage.Workbook.Worksheets.Add(fileName);

                    //Load the CSV data into the Excel worksheet
                    //worksheet.Cells["A1"].LoadFromText(csvFile, new ExcelTextFormat { Delimiter = ',' });
                    worksheet.Cells["A1"].LoadFromCollection(excelentries, true);

                    //Formatting to custom column name, will work on this to make it dynamic later, Incase total no. of column changes.
                    worksheet.Cells["A1"].Value = "DEPOSITOR NAME";
                    worksheet.Cells["B1"].Value = "BANK REFERENCE NO.";
                    worksheet.Cells["C1"].Value = "TRANSACTION DATE";
                    worksheet.Cells["D1"].Value = "AMOUNT";
                    worksheet.Cells["E1"].Value = "ORIGINATOR BANK";
                    worksheet.Cells["F1"].Value = "DESTINATION BANK";
                    worksheet.Cells["G1"].Value = "DEBIT ACCOUNT NAME";
                    worksheet.Cells["H1"].Value = "CREDIT ACCOUNT NAME";
                    worksheet.Cells["I1"].Value = "DEBIT ACCOUNT";
                    worksheet.Cells["J1"].Value = "CREDIT ACCOUNT";
                    worksheet.Cells["K1"].Value = "REMARKS";
                    worksheet.Cells["L1"].Value = "TRANSACTION CHANNEL";

                    //Apply formatting to the Excel worksheet
                    //First implemented a dynamic retreival of total length of excel columns using the 'CSVDetails' Class.
                    PropertyInfo[] columnProperty = typeof(CSVDetails).GetProperties();
                    int columnLength = columnProperty.Length;

                    //Get the Excel Column Name
                    var columName = await GetCSVColumnName(destination, columnLength);
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    columName.Logs.AddToLogs(ref logs);

                    //var headerRange = worksheet.Cells["A1:" + columnLength + "1"];
                    var headerRange = worksheet.Cells["A1:" + columName.objectValue + "1"];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    headerRange.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    //Implementation to make the column width fit with the column name / reduce overlapping.
                    for (int columnIndex = 1; columnIndex <= columnLength; columnIndex++)
                    {
                        var columnName = await GetCSVColumnName(destination, columnIndex);
                        var column = worksheet.Column(columnIndex);

                        PropertyInfo specificProperty = columnProperty[columnIndex - 1];//Subtract because PROPERTY index begins from 0
                        int maxLength = ConfigSettings.WebConfigAttributes.ExccelColumnWidth;
                        if (specificProperty.Name.ToLower() == "remark")
                        {
                            // Adjust the width with some buffer(50)
                            worksheet.Column(11).Width = (maxLength + 50);
                        }
                        else if (specificProperty.Name == "BankReferenceNo")
                        {
                            column.AutoFit(maxLength + 10);
                            //worksheet.Column(11).Width = (maxLength + 10);
                            var referenceDataRange = worksheet.Cells[$"{columnName.objectValue}2:{columnName.objectValue}:{excelentries.Count + 1}"];
                            referenceDataRange.Style.Numberformat.Format = "0";
                        }
                        else
                        {
                            column.AutoFit(maxLength + 5);// Adjust the width with some buffer
                        }

                    }

                    //Implementation to format the rows
                    var dataRange = worksheet.Cells["A2:" + columName.objectValue + excelentries.Count + 1];
                    dataRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    dataRange.Style.WrapText = true;

                    //Save the modified Excel file
                    string location = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.FormattedFilePath}");

                    if (!Directory.Exists(location))
                    {
                        Directory.CreateDirectory(location);
                    }
                    excelPackage.SaveAs(new FileInfo(Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.FormattedFilePath}/{fileName}.csv")));

                    //Encrypt the Saved File (With Password)
                    if (bool.Parse(ConfigSettings.WebConfigAttributes.LockFilesWithPassword))
                    {
                        FileEncryptor.EncrypFile(fileName);
                        //Clean up temporary files
                        File.Delete(Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.FormattedFilePath}/{fileName}.csv"));
                    }

                    // Clean up temporary files
                    File.Delete(Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.RawFilesPath}/{fileName}.csv"));
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} File: {fileName} sucessfully generated and formatted.").AppendLine();
                }

            }
            catch( Exception ex )
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception occured").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                return new DatabaseResult<bool>
                {
                    Logs = logs,
                    objectValue = false
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            return new DatabaseResult<bool>
            {
                Logs = logs,
                objectValue = true
            };
        }

        public async Task<DatabaseResult<string>> GetCSVColumnName(string destination, int columnNumber)
        {
            string classMeth = "GetCSVColumnName";
            var theReturner = new DatabaseResult<bool>();
            string columnName = string.Empty;

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            try
            {
                int dividend = columnNumber;                

                while (dividend > 0)
                {
                    /*Since Column Name exceeds A-Z, the implementation will help to accomodate higher
                    column Names like AB AZ and so on. */
                    int modulo = (dividend - 1) % 26;

                    //Getting the ASCII rep. of the resulting column
                    columnName = Convert.ToChar(65 + modulo) + columnName;

                    dividend = (dividend - modulo) / 26;
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Retreived Column Name is: {columnName}").AppendLine();
                }
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception occured").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                return new DatabaseResult<string>
                {
                    Logs = logs,
                    objectValue = columnName
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            return new DatabaseResult<string>
            {
                Logs = logs,
                objectValue = columnName
            };
        }

        public async Task<DatabaseResult<bool>> WriteToCSV(string destination, List<CSVDetails> excelEntries, string fileName)
        {
            string classMeth = "GenerateExcelSheet";
            var theReturner = new DatabaseResult<bool>();

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            try
            {
                string location = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.RawFilesPath}");

                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                var csvPath = Path.Combine(Environment.CurrentDirectory, $"{ConfigSettings.WebConfigAttributes.RawFilesPath}/{fileName}.csv");
                using (var streamWriter = new StreamWriter(csvPath))
                {
                    using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
                    {
                        csvWriter.Context.RegisterClassMap<CSVColumnMapper>();
                        csvWriter.WriteRecords(excelEntries);
                    }
                }
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} CSV successfully generated on the path: {csvPath}").AppendLine();
            }
            catch ( Exception ex )
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception occured").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                return new DatabaseResult<bool>
                {
                    Logs = logs,
                    objectValue = false
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                
            }

            return new DatabaseResult<bool>
            {
                Logs = logs,
                objectValue = true
            };
        }
        
    }
}
