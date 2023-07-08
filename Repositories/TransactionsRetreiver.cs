using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using PTAUpdater.Helpers.Extensions;
using PTAUpdater.Helpers.Global;
using PTAUpdater.Helpers.Models;
using PTAUpdater.Helpers.Services;
using PTAUpdater.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PTAUpdater.Helpers.Common.Utils;
using PTAUpdater.Helpers.Common;

namespace PTAUpdater.Repositories
{
    public class TransactionsRetreiver : ITransactionsRetreiver
    {
        private readonly ISQLBinary _dataBaseAgent;
        private readonly ICommunicationHandler _MPNClient;
        public TransactionsRetreiver(ISQLBinary dataBaseAgent, ICommunicationHandler MPNClient)
        {
            _dataBaseAgent = dataBaseAgent;
            _MPNClient = MPNClient;
        }
        public async Task<TransReturnObject> ProcessBANK(string destination, DateObject dates, string Type)
        {
            var excelEntries = new List<CSVDetails>();
            string classMeth = "ExtractGTTransactionsFromBASIS";
            int counter = 0;

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------\r\n").AppendLine();

            string query = (Type.ToLower() == "debit") ? DapperQueries.QueryDebitTrans : DapperQueries.QueryCreditTrans;
            string querySecondLeg  = (query == DapperQueries.QueryDebitTrans) ? DapperQueries.QueryDebitSecondLeg : DapperQueries.QueryCreditSecondLeg;
            string[] oldPTAAccounutSplitted = ConfigSettings.WebConfigAttributes.PTAAccountsOld.Split(',', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                foreach (var account in oldPTAAccounutSplitted)
                {
                    
                    string? PTANuban = account switch //Implememnted to reduce round trip to database...
                    {
                        "8745/89875485/587/4/877" => "0019586217",
                        "8745/89875485/587/4/877" => "0176802498",
                        "8745/89875485/587/4/877" => "0026740398",
                        _ => (await _dataBaseAgent.ConvertToNuban(destination, account))?.objectValue,//Incase new PTA Account is Added.
                    };
                    //string? PTANuban = (await _dataBaseAgent.ConvertToNuban(destination, account))?.objectValue;

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About retreiving {Type} transactions first leg for account {account} from basis").AppendLine();
                    var entries = JsonConvert.SerializeObject(GenerateOldAccount.GetAccountAttributes(account));
                    var retrievedTransFirstLeg = await _dataBaseAgent.SqlSelectQueryBasisWithParams(classMeth, "", query, CommandType.Text, null, dates, entries, 1);
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} {retrievedTransFirstLeg.objectValue?.Rows.Count} transactions retreived from basis for first leg").AppendLine();

                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    retrievedTransFirstLeg.Logs.AddToLogs(ref logs);

                    if (retrievedTransFirstLeg.queryIsSuccessful && retrievedTransFirstLeg.objectValue?.Rows.Count > 0)
                    {
                        foreach(DataRow transaction in retrievedTransFirstLeg.objectValue.Rows)
                        {
                            decimal amount = decimal.Parse(transaction["tra_amt"].ToString());
                            string remarks = transaction["remarks"].ToString();
                            string doc_alp = transaction["doc_alp"].ToString();
                            int Origt_tra_seq1 = int.Parse(transaction["Origt_tra_seq1"].ToString());
                            DateTime Origt_tra_date = DateTime.Parse(transaction["Origt_tra_date"].ToString());
                           string transactionDate = transaction["tra_date"].ToString();
                           string origt_bra_code = transaction["origt_bra_code"].ToString();
                           string origt_tra_seq1 = transaction["origt_tra_seq1"].ToString();

                            //logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About retreiving {Type} transactions second leg for account {account} with doc_alp {doc_alp}").AppendLine();
                            var secondLegEntriesObject = new TransSecondLeg
                            {
                                origt_bra_code = origt_bra_code,
                                origt_tra_seq1 = origt_tra_seq1,
                                amount = amount,
                                date = Origt_tra_date
                            };

                            var secondLegEntries = JsonConvert.SerializeObject(secondLegEntriesObject);
                            
                            var retrievedTransSecondLeg = await _dataBaseAgent.SqlSelectQueryBasisWithParams(classMeth, ConfigSettings.ConnectionString.DecryptedBasisDbConection, querySecondLeg, CommandType.Text, null, dates, secondLegEntries, 2);
                            //logBuilder.AppendLine($"{retrievedTransSecondLeg.objectValue?.Rows.Count} transactions retreived from basis for second leg").AppendLine();

                            //Retry incase the first process fails, there should always be a second leg.
                            if (retrievedTransSecondLeg.objectValue?.Rows.Count == 0)
                            {
                                var retrievedTransSecondLeg1 = await _dataBaseAgent.SqlSelectQueryBasisWithParams(classMeth, ConfigSettings.ConnectionString.DecryptedBasisDbConection, querySecondLeg, CommandType.Text, null, dates, secondLegEntries, 2);
                                retrievedTransSecondLeg = (retrievedTransSecondLeg.objectValue?.Rows.Count == 0) ? retrievedTransSecondLeg1 : retrievedTransSecondLeg;
                            }
                            logBuilder.ToString().AddToLogs(ref logs);
                            logBuilder.Clear();
                            retrievedTransSecondLeg.Logs.AddToLogs(ref logs);

                            //logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About Checking if this transaction was via MPN").AppendLine();
                            string[] MPNLedgers = ConfigSettings.WebConfigAttributes.MPNLedgers.Split(",");
                            var getPotentialMPNLedger = GenerateOldAccount.GetOldAccountSecondLeg(retrievedTransSecondLeg.objectValue);
                            string potentialMPNLedger = $"{getPotentialMPNLedger.bra_code}/{getPotentialMPNLedger.cus_num}/{getPotentialMPNLedger.cur_code}/{getPotentialMPNLedger.led_code}/{getPotentialMPNLedger.sub_acct_code}";

                            if (!MPNLedgers.Any(s => s.Contains(potentialMPNLedger)))
                            {
                                //logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Current transaction is not an MPN trans, about adding to the list of transactions for intra bank").AppendLine();

                                //Caching mechanism to reduce round trip to database.
                                var cachedCustomerNuban = CachingUtility.GetValueFromCache(potentialMPNLedger);
                                string? customerNuban = (cachedCustomerNuban == null) ? (await _dataBaseAgent.ConvertToNuban(classMeth, potentialMPNLedger)).objectValue : cachedCustomerNuban;

                                var cachedPTALedgerName = CachingUtility.GetValueFromCache(PTANuban);
                                string? PTALedgerName = (cachedPTALedgerName == null) ? (await _dataBaseAgent.GetCustomerName(classMeth, PTANuban))?.objectValue : cachedPTALedgerName;

                                var cachedcustomerLedgerName = CachingUtility.GetValueFromCache(customerNuban);
                                string? customerLedgerName = (cachedcustomerLedgerName == null) ? (await _dataBaseAgent.GetCustomerName(classMeth, customerNuban))?.objectValue : cachedcustomerLedgerName;

                                string? depositor = (Type.ToLower() == "debit") ? PTALedgerName : customerLedgerName;

                                var transactionRecord = new CSVDetails
                                {
                                    DepositorName = depositor,
                                    BankReferenceNo = ReferenceGenerator.GetTransactionReference(remarks, doc_alp),
                                    TransactionDate = transactionDate,
                                    Amount = amount.ToString(),
                                    OriginatorBank = ConfigSettings.WebConfigAttributes.BankName,
                                    DestinationBank = ConfigSettings.WebConfigAttributes.BankName,
                                    DebitAccountName = (Type.ToLower() == "debit") ? PTALedgerName : customerLedgerName,
                                    CreditAccountName = (Type.ToLower() == "debit") ? customerLedgerName : PTALedgerName,
                                    DebitAccount = (Type.ToLower() == "debit") ? PTANuban : customerNuban,
                                    CreditAccount = (Type.ToLower() == "debit") ? customerNuban : PTANuban,
                                    Remark = ReferenceGenerator.RemoveUnwantedSpaces(remarks),
                                    TransactionType = "Others",
                                };

                                excelEntries.Add(transactionRecord);
                                counter ++;
                            }
                            else
                            {
                                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Current transaction is MPN, proceeding to next transaction...").AppendLine();
                                continue;
                            }
                        }
                    }
                    else
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} No transactions found for account {account} on basis, proceeding to next account...").AppendLine();
                        continue;
                    }

                }
            }
            catch(Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception occured").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                var exceptionObject = new TransReturnObject
                {
                    Logs = logs,
                    ExcelEntries = excelEntries,
                };
                //Task.Run(() => LogWriter.WriteLog(logs));
                return exceptionObject;
            }
            finally
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} {counter} BANK {Type} transactions retreived and prepared to be sent to PTA.").AppendLine();
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            var returnObject = new TransReturnObject
            {
                Logs = logs,
                ExcelEntries = excelEntries,
            };
            //Task.Run(() => LogWriter.WriteLog(logs));
            return returnObject;
        }

        public async Task<TransReturnObject> ProcessMPN(string destination, DateObject dates, string type)
        {
            var excelEntries = new List<CSVDetails>();
            string classMeth = "FetchMPNTransactions";
            int counter = 0;

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------Switched from {destination} to {classMeth}----------------").AppendLine();
            string[] PTAAccounutSplitted = ConfigSettings.WebConfigAttributes.PTAAccountsNuban.Split(',', StringSplitOptions.RemoveEmptyEntries);

            string NIBBSUrl = (type.ToLower() == "debit") ? ConfigSettings.WebConfigAttributes.MPNOutward : ConfigSettings.WebConfigAttributes.MPNInward;

            try
            {
                foreach (var account in PTAAccounutSplitted)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Request sent to MPN to retreive {type} transactions for the PTA account number: {account}").AppendLine();

                    ObjectMultiSelect parameterArray = new ObjectMultiSelect();
                    parameterArray.sItem = new string[3];
                    parameterArray.sValue = new string[3];
                    parameterArray.sItem[0] = "DateFrom";
                    parameterArray.sValue[0] = dates.queryStartDate.ToString();
                    parameterArray.sItem[1] = "DateTo";
                    parameterArray.sValue[1] = dates.queryEndDate.ToString();
                    parameterArray.sItem[2] = "BeneAccount";
                    parameterArray.sValue[2] = account;

                    var MPNResponse = await _MPNClient.HttpPostMethod(classMeth, NIBBSUrl, parameterArray);
                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();
                    MPNResponse.Logs.AddToLogs(ref logs);

                    if (MPNResponse.handShakeIsSuccessful && !string.IsNullOrEmpty(MPNResponse.objectValue))
                    {
                        
                        ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(MPNResponse.objectValue);                        

                        string statuscode = apiResponse.responseCode;
                        if (statuscode == "00")
                        {
                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Response from MPN was sucessfull").AppendLine();
                            for (int i = 0; i < apiResponse.searchResults.Length; i++)
                            {
                                //string thisDebitAccount = (type.ToLower() == "debit") ? apiResponse.searchResults[i].accountNumber : apiResponse.searchResults[i].beneAccountNumber;
                                //string thisCreditAccount = (type.ToLower() == "debit") ? apiResponse.searchResults[i].beneAccountNumber : apiResponse.searchResults[i].accountNumber;
                                //string thisDebitAccountName = (type.ToLower() == "debit") ? apiResponse.searchResults[i].accountName : apiResponse.searchResults[i].beneAccountName;
                                //string thisCreditAccountName = (type.ToLower() == "debit") ? apiResponse.searchResults[i].beneAccountName : apiResponse.searchResults[i].accountName;

                                var transactionRecord = new CSVDetails
                                {
                                    DepositorName = apiResponse.searchResults[i].accountName,
                                    BankReferenceNo = apiResponse.searchResults[i].sessionId,
                                    TransactionDate = apiResponse.searchResults[i].transactionDate.ToString(),
                                    Amount = apiResponse.searchResults[i].amount.ToString(),
                                    OriginatorBank = apiResponse.searchResults[i].originatingBankName,
                                    DestinationBank = apiResponse.searchResults[i].destinationBankName,
                                    DebitAccountName = apiResponse.searchResults[i].accountName,/*thisDebitAccountName,*/
                                    CreditAccountName = apiResponse.searchResults[i].beneAccountName,//thisCreditAccountName,
                                    DebitAccount = apiResponse.searchResults[i].accountNumber,//thisDebitAccount,
                                    CreditAccount = apiResponse.searchResults[i].beneAccountNumber,//thisCreditAccount,
                                    Remark = ReferenceGenerator.RemoveUnwantedSpaces(apiResponse.searchResults[i].remark),
                                    TransactionType = "NIBSS-MPN",
                                };

                                excelEntries.Add(transactionRecord);
                                counter++;
                            }
                        }
                        else
                        {
                            string ret_str = apiResponse.responseDescription;
                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} API request to get records from MPN failed with response code {statuscode} and response description {ret_str}").AppendLine();
                        }
                    }
                    else
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An error occured when fetching {type} transactions for the PTA account number: {account}, Proceeding to next account....").AppendLine();
                        continue;
                    }

                }
            }
            catch(Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LogType.LOG_DEBUG, ref logs, ex, classMeth + " Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} An exception occured").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();

                var exceptionObject = new TransReturnObject
                {
                    Logs = logs,
                    ExcelEntries = excelEntries,
                };
                //Task.Run(() => LogWriter.WriteLog(logs));
                return exceptionObject;
            }
            finally
            {
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} {counter} MPN {type} transactions retreived and prepared to be sent to PTA.").AppendLine();
                logBuilder.AppendLine($"--------------Switching back to {destination}----------------\r\n\r\n").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            var returnObject = new TransReturnObject
            {
                Logs = logs,
                ExcelEntries = excelEntries,
            };
            //Task.Run(() => LogWriter.WriteLog(logs));
            return returnObject;
        }
    }
}
