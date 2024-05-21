using System.Linq;
using System.Text;

namespace jsiSIE
{
    public class EncodingHelper
    {
        
        private static int _default = 28591; //ISO-8859-1 (single byte fallback)
        public static Encoding GetDefault(){
            if(_default == 0)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var encodings = Encoding.GetEncodings();

                if(encodings.Any(e => e.CodePage == 437)){
                    _default = 437; //PC-8 CP437 (SIE standard)
                } 
            }

            return Encoding.GetEncoding(_default);
        }
    }
}