using PTAUpdater.Helpers.Common;
using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.ConnectionManager;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Helpers.Services;
using PTAUpdater.Interfaces;
using BANKEncryptLibrary;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;

namespace PTAUpdater.Repositories
{
    public class SQLBinary : ISQLBinary
    {
        public async Task<DatabaseResult<string>> ConvertToNuban(string destination, string oldAccountNumber)
        {
            string classMeth = "convertToNuban";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            char acctsplit = Convert.ToChar("/");
            string[] accountkey = new string[4];
            string bra_code = string.Empty;
            string cus_num = string.Empty;
            string cur_code = string.Empty;
            string led_code = string.Empty;
            string sub_acct_code = string.Empty;

            accountkey = oldAccountNumber.Trim().Split(acctsplit);
            bra_code = accountkey[0];
            cus_num = accountkey[1];
            cur_code = accountkey[2];
            led_code = accountkey[3];
            sub_acct_code = accountkey[4];

            int? result = null;
            string NUBAN = string.Empty;

            string selectquery = "select  MAP_ACC_NO from map_acct where bra_code = " + bra_code + " and cus_num = " + cus_num + "and cur_code = " + cur_code + "and led_code = " + led_code + " and sub_acct_code = " + sub_acct_code;

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About Opening Oracle connection to execute {selectquery}").AppendLine();
            using (OracleConnection OraConn = SqlManager.OracleDatabaseCreateConnection(ConfigSettings.ConnectionString.DecryptedBasisDbConection, true))
            {
                using OracleCommand OraSelectb = new OracleCommand();
                {
                    try
                    {
                        OraSelectb.Connection = OraConn;
                        OraSelectb.CommandText = selectquery;
                        OraSelectb.CommandType = CommandType.Text;

                        NUBAN = await DapperUtility<string>.GetSingleObjectAsync(OraConn, selectquery, CommandType.Text);

                        if (!string.IsNullOrEmpty(NUBAN))
                        {
                            CachingUtility.AddValueToCache(oldAccountNumber, NUBAN);
                        }
                    }
                    catch (Exception ex)
                    {
                        //ON EXCEPTION STORE THE PREVIOUS LOG
                        LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_INFORMATION, ref logs, ex, classMeth + " Exception");
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception Occured when Converting Old Account to Nuban").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);                        

                        return new DatabaseResult<string>
                        {
                            Logs = logs,
                            objectValue =   ConfigSettings.WebConfigAttributes.DefaultAccountNumberWhenErrorOccurs,
                            queryIsSuccessful = false
                        };
                    }
                    finally
                    {
                        SqlManager.closeOpenedConnection1(OraConn);
                        logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();
                        //Task.Run(() => LogWriter.WriteLog(logs));
                    }
                }
            }
            return new DatabaseResult<string>
            {
                objectValue = (result != 0) ? NUBAN : DefaultAccount,
                queryIsSuccessful = (result != 0) ? true : false,
                Logs = logs
            };
        }

        public async Task<DatabaseResult<string>> GetCustomerName(string destination, string Nuban)
        {
            string classMeth = "GetCustomerName";
            string customerName = null;
            int result;
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            string selectquery = "select get_name(bra_code, cus_num,0,0,0) Name from map_acct where map_acc_no =" + Nuban + "";

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to open BASIS to execute {selectquery}").AppendLine();
            using (OracleConnection OraConn = SqlManager.OracleDatabaseCreateConnection(ConfigSettings.ConnectionString.DecryptedBasisDbConection, true))
            {
                using OracleCommand OraSelectb = new OracleCommand();
                {
                    try
                    {
                        OraSelectb.Connection = OraConn;
                        OraSelectb.CommandText = selectquery;
                        OraSelectb.CommandType = CommandType.Text;
                        
                        customerName = await DapperUtility<string>.GetSingleObjectAsync(OraConn, selectquery, CommandType.Text);

                        if (!string.IsNullOrEmpty(customerName))
                        {
                            CachingUtility.AddValueToCache(Nuban, customerName);
                        }
                    }
                    catch (Exception ex)
                    {
                        //ON EXCEPTION STORE THE PREVIOUS LOG
                        LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_INFORMATION, ref logs, ex, classMeth + " Exception");
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An Error Occured when Retreiving Customer Name from Basis").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        

                        return new DatabaseResult<string>
                        {
                            Logs = logs,
                            objectValue = DefaultCustomer,
                            queryIsSuccessful = false
                        };
                    }
                    finally
                    {
                        SqlManager.closeOpenedConnection1(OraConn);
                        logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();
                    }
                }
            }
            return new DatabaseResult<string>
            {
                objectValue = customerName,
                queryIsSuccessful = true,
                Logs = logs
            };
        }        

        public async Task<DatabaseResult<DataTable>> SqlSelectQueryBasisWithParams(string destination, string ConnString, string CommandQuery, CommandType cmdType, SqlParameter[] param, DateObject dates, object entries, int type)
        {
            string classMeth = "SelectRecordsOnBasis";
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n");
            int result = 0;
            DataTable ds = new DataTable();
            var returnObject = new DatabaseResult<DataTable>();

            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to open Oracle connection to execute {CommandQuery}").AppendLine();
            using (OracleConnection OraConn = SqlManager.OracleDatabaseCreateConnection(ConfigSettings.ConnectionString.DecryptedBasisDbConection, true))
            {
                using OracleCommand OraSelect = new OracleCommand();
                {
                    try
                    {
                        OraSelect.Connection = OraConn;
                        OraSelect.CommandText = CommandQuery;
                        OraSelect.CommandType = CommandType.Text;

                        var paras = new Dictionary<string, DapperParameterWrapper>();
                        if (param != null)
                        {
                            OraSelect.Parameters.AddRange(param);
                        }
                        else if (type == 1)
                        {
                            var oldAccountObject = JsonConvert.DeserializeObject<OldAccountMembers>((string)entries);
                                                        
                            paras.Add("bra_code", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.bra_code), DbType.Int32));
                            paras.Add("cus_num", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.cus_num), DbType.Int32));
                            paras.Add("cur_code", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.cur_code), DbType.Int32));
                            paras.Add("led_code", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.led_code), DbType.Int32));
                            paras.Add("sub_acct_code", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.sub_acct_code), DbType.Int32));
                            paras.Add("queryStartDate", new DapperParameterWrapper(dates.queryStartDate, DbType.DateTime));
                            paras.Add("queryEndDate", new DapperParameterWrapper(dates.queryEndDate, DbType.DateTime));
                            
                        }
                        else if (type == 2)
                        {
                            var oldAccountObject = JsonConvert.DeserializeObject<TransSecondLeg>((string)entries);

                            string queryDate = oldAccountObject.date.ToString("ddMMMyyyy").ToLower();
                            CommandQuery = CommandQuery.Replace(":origt_tra_date", queryDate);

                            paras.Add("origt_bra_code", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.origt_bra_code), DbType.Int32));
                            paras.Add("origt_tra_seq1", new DapperParameterWrapper(Convert.ToInt32(oldAccountObject.origt_tra_seq1), DbType.Int32));
                            //paras.Add("tra_amt", new DapperParameterWrapper(Convert.ToDecimal(oldAccountObject.amount), DbType.Decimal));

                        }

                        ds = await DapperUtility<DataTable>.GetObjectAsync(OraConn, paras, CommandQuery, CommandType.Text);
                        returnObject.queryIsSuccessful = (ds != null) ? true : false;
                        returnObject.objectValue = ds;
                        
                    }
                    catch (Exception ex)
                    {
                        //ON EXCEPTION STORE THE PREVIOUS LOG
                        LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An Error Occured While Trying to Retreive Records from Basis").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();

                        return new DatabaseResult<DataTable>
                        {
                            Logs = logs,
                            objectValue = null,
                            queryIsSuccessful = false
                        };
                    }
                    finally
                    {
                        SqlManager.closeOpenedConnection1(OraConn);
                        logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();
                        returnObject.Logs = logs;
                    }
                }
            }            
            return returnObject;
        }
    }
}
