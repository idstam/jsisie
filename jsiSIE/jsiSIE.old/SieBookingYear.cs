using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    /// <summary>
    /// #RAR
    /// </summary>
    [Serializable]
    public class SieBookingYear
    {
        public int ID { get; set; }
        public DateTime? Start { get; set;}
        public DateTime? End {get; set;}
    }
}
