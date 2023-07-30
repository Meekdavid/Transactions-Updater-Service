using PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Services
{
    public static class Salutation
    {
        public static string EmailSalutationFooter()
        {
            string theReturner = string.Empty;

            DateTime currentDate = DateTime.Today;

            // Check if it's between 15th December and 30th January
            if (currentDate >= new DateTime(currentDate.Year, 12, 15) && currentDate <= new DateTime(currentDate.Year, 1, 30))
            {
                // Check for specific dates
                if (currentDate == new DateTime(currentDate.Year, 12, 25))
                {
                    theReturner = "Merry Xmas!";
                }
                else if (currentDate == new DateTime(currentDate.Year, 1, 1))
                {
                    theReturner = "Happy New Year!";
                }
                else
                {
                    theReturner = "Compliments of the season!";
                }
            }
            // Check for other holidays or celebrations
            else if (currentDate == new DateTime(currentDate.Year, 5, 1))
            {
                theReturner = "Happy Workers' Day!";
            /}
            else if (IsEidAlFitr(currentDate))
            {
                theReturner = "Eid Mubarak!";
            }
            else
            {
                // Return a default greeting
                theReturner = ConfigSettings.WebConfigAttributes.EmailFooter;
            }

            return theReturner;
        }

        private static bool IsEidAlFitr(DateTime date)
        {
            // Eid al-Fitr date calculation - Example: 2023
            int year = date.Year;
            DateTime ramadanStartDate = new DateTime(year, 4, 13);
            DateTime ramadanEndDate = ramadanStartDate.AddDays(29);

            return date >= ramadanEndDate && date <= ramadanEndDate.AddDays(1);
        }
    }
}
