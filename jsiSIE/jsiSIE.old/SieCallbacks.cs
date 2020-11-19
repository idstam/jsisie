using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    public class SieCallbacks
    {
        public Action<string> Line;
        public void CallbackLine(string message)
        {
            if(Line != null) Line(message);
        }

        public Action<Exception> SieException;
        public void CallbackException(Exception ex)
        {
            if(SieException != null) SieException(ex);
        }

        public Action<SiePeriodValue> IB;
        internal void CallbackIB(SiePeriodValue pv)
        {
            if (IB != null) IB(pv);
        }

        public Action<SiePeriodValue> UB;
        internal void CallbackUB(SiePeriodValue pv)
        {
            if (UB != null) UB(pv);
        }

        public Action<SiePeriodValue> OIB;
        internal void CallbackOIB(SiePeriodValue pv)
        {
            if (OIB != null) OIB(pv);
        }

        public Action<SiePeriodValue> OUB;
        internal void CallbackOUB(SiePeriodValue pv)
        {
            if (OUB != null) OUB(pv);
        }

        public Action<SiePeriodValue> PSALDO;
        internal void CallbackPSALDO(SiePeriodValue pv)
        {
            if (PSALDO != null) PSALDO(pv);
        }

        public Action<SiePeriodValue> PBUDGET;
        internal void CallbackPBUDGET(SiePeriodValue pv)
        {
            if (PBUDGET != null) PBUDGET(pv);
        }

        public Action<SiePeriodValue> RES;
        internal void CallbackRES(SiePeriodValue pv)
        {
            if (RES != null) RES(pv);
        }

        public Action<SieVoucher> VER;
        internal void CallbackVER(SieVoucher v)
        {
            if (VER != null) VER(v);
        }
    }
}
