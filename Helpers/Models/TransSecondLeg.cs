using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Models
{
    public class TransSecondLeg
    {
        public string origt_bra_code { get; set; }
        public string origt_tra_seq1 { get; set; }
        public decimal amount { get; set; }
        public DateTime date { get; set; }
    }
}
