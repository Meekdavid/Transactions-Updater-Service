using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Services
{
    public static class ReferenceGenerator
    {
        public static string GetTransactionReference(string remarks, string doc_alp)
        {
            string theReturner = string.Empty;

            try
            {
                string pattern = doc_alp switch
                {
                    "MPNU" => @"(?<=REF:)\d+",
                    "USAT" => @"USSD-(\d+)-",
                    "MPNT" => @"\d{30}",
                    "USGT" => @"\d{32}",
                    "GWTR" => @"\d{30}",
                    "MPNG" => @"GW\d{30}",
                    "GTCN" => @"\d{20}P\d{21}",
                    _ => @"\d+",
                };

                Match match = Regex.Match(remarks, pattern);

                if (match.Success)
                {
                    if (doc_alp == "USAT")
                    {
                        theReturner = match.Groups[1].Value;
                    }
                    else
                    {
                        theReturner = match.Value;
                    }
                }
                else
                {
                    theReturner = GetRandomReference(remarks);
                }
            }
            catch (Exception ex)
            {
                theReturner = GetRandomReference(remarks);
            }

            return theReturner;
        }

        public static string GetRandomReference(string remarks)
        {
            string theReturner = "";
            string pattern = @"\d+";

            try
            {
                Match match = Regex.Match(remarks, pattern);

                if (match.Success)
                {
                    theReturner = match.Value;
                }
                else
                {
                    theReturner = GetRandomReferenceInt(10);
                }
            }
            catch (Exception ex)
            {
                theReturner = GetRandomReferenceInt(10);
            }

            return theReturner;
        }

        public static string GetRandomReferenceInt(int value)
        {
            Random myRandom = new Random();
            int minimo = (int)Math.Pow(10, value - 6);
            int maximo = (int)Math.Pow(10, value) - 1;

            return "AUTO" + (myRandom.Next(minimo, maximo)).ToString();
        }
        public static string RemoveUnwantedSpaces(string remarks)
        {
            string theReturner = string.Empty;

            try
            {
                string pattern = @"\s{4,}";
                string replacement = " ";
                theReturner = Regex.Replace(remarks, pattern, replacement);
            }
            catch (Exception ex)
            {
                theReturner = remarks;
            }

            return theReturner;
        }
    }
}
