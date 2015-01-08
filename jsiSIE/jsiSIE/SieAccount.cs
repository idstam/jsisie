using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    /// <summary>
    /// #KONTO
    /// </summary>
    [Serializable]
    public class SieAccount
    {
        public SieAccount()
        {
            SRU = new HashSet<string>();
        }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public HashSet<string> SRU { get; set; }
    }
}
