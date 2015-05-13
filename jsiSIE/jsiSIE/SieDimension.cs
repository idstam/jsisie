
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    [Serializable]
    public class SieDimension
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public bool IsDefault = false;

        private SieDimension _parent = null;
        public SieDimension SuperDim 
        { 
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                _parent.SubDim.Add(this);
            } 
        }
        public HashSet<SieDimension> SubDim = new HashSet<SieDimension>();
        public Dictionary<string, SieObject> Objects = new Dictionary<string, SieObject>();
        
    }
}
