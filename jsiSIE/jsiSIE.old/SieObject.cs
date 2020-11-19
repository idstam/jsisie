using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    [Serializable]
    public class SieObject    
    {
        public SieDimension Dimension { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }

        
    }
}
