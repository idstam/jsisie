using System.Linq;
using System.Text;

namespace jsiSIE
{
    public class EncodingHelper
    {
        
        private static int _default = 0;
        public static Encoding GetDefault(){
            if(_default == 0)
            {
                //Dotnet core does not have codepage 437 that is the standard for SIE
                //I fall back to ISO-8859-1 as that is a single byte code page so that KSUMMA can be calculated.
                var encodings = Encoding.GetEncodings();
                if(encodings.Any(e => e.CodePage == 437)){
                    _default = 437;
                } else {
                    _default = 28591;
                }
            }

            return Encoding.GetEncoding(_default);
        }
    }
}