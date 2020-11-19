using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    [Serializable]
    public class SieVoucherRow
    {
        public SieAccount Account { get; set; }
        public List<SieObject> Objects { get; set; }
        public decimal Amount { get; set; }
        public DateTime RowDate { get; set; }
        public string Text { get; set; }
        public decimal? Quantity;
        public string CreatedBy { get; set; }
        public string Token { get; set; }
    }
}
