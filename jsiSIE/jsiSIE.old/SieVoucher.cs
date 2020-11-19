using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    [Serializable]
    public class SieVoucher
    {
        public string Series { get; set; }
        public string Number { get; set; }
        public DateTime VoucherDate { get; set; }
        public string Text { get; set; }
        public int CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Token { get; set; }

        public List<SieVoucherRow> Rows { get; set; }

        public SieVoucher()
        {
            Rows = new List<SieVoucherRow>();
        }
    }
}
