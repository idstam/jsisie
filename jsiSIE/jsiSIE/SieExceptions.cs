using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{

    public class SieInvalidChecksumException : Exception
    {
        public SieInvalidChecksumException(string description) : base(description) { }
    }

    public class SieInvalidFileException : Exception
    {
        public SieInvalidFileException(string description) : base(description) { }
    }

    public class SieDateException : Exception 
    {
        public SieDateException(string description) : base(description) { }
    }

    public class SieInvalidFeatureException : Exception
    {
        public SieInvalidFeatureException(string description) : base(description) { }
    }

    public class SieMissingMandatoryDateException : Exception
    {
        public SieMissingMandatoryDateException(string description) : base(description) { }
    }

    public class SieMissingObjectException : Exception
    {
        public SieMissingObjectException(string description) : base(description) { }
    }

    public class SieVoucherMissmatchException : Exception
    {
        public SieVoucherMissmatchException(string description) : base(description) { }
    }

}
