using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.ConfigurationSettinigs.AppSettings
{
    public class Connectionstrings
    {
        public string GTCNDbConection { get; set; }
        public string gTCNDbConection { get; set; }
        public string DecryptedGTCNDbConection
        {
            get
            {
                gTCNDbConection = BANKEncryptLibrary.BANKEncryptLib.DecryptText(GTCNDbConection);
                return gTCNDbConection;
            }
            set
            {
                gTCNDbConection = value;
            }
        }
        public string BasisDbConection { get; set; }
        public string basisDbConection { get; set; }

        public string DecryptedBasisDbConection
        {
            get
            {
                basisDbConection = BANKEncryptLibrary.BANKEncryptLib.DecryptText(BasisDbConection);
                return basisDbConection;
            }
            set
            {
                basisDbConection= value;
            }
        }
    }
}
