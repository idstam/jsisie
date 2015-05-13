using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    public class SieDocumentWriter
    {
        private SieDocument _sie;
        private Stream _stream;
        private Encoding _encoding;

        public SieDocumentWriter(SieDocument sie)
        {
            _sie = sie;
            _encoding = Encoding.GetEncoding(437);
        }


        public void Write(string file)
        {
            if (File.Exists(file)) File.Delete(file);
            _stream = File.OpenWrite(file);

            WriteLine(FLAGGA);
            WriteLine(PROGRAM);
            WriteLine(FORMAT);
            WriteLine(GEN);
            WriteLine(SIETYP);
            if(!string.IsNullOrEmpty(_sie.PROSA)) WriteLine("#PROSA " + "\"" + _sie.PROSA + "\"");
            WriteFNR();
            WriteLine(ORGNR);
            WriteLine(FNAMN);
            WriteADRESS();
            WriteFTYP();
            WriteKPTYP();
            WriteVALUTA();
            WriteTAXAR();
            WriteOMFATTN();
            WriteRAR();

            WriteDIM();
            WriteKONTO();

            WritePeriodValue("#IB", _sie.IB);
            WritePeriodValue("#UB", _sie.UB);
            if (_sie.SIETYP >= 3)
            {
                WritePeriodValue("#OIB", _sie.OIB);
                WritePeriodValue("#OUB", _sie.OUB);
            }
            if (_sie.SIETYP > 1)
            {
                WritePeriodSaldo("#PBUDGET", _sie.PBUDGET);
                WritePeriodSaldo("#PSALDO", _sie.PSALDO);
            }
            WritePeriodValue("#RES", _sie.RES);
            WriteVER();
            
            _stream.Close();
            _stream.Dispose();

            

        }


        private void WriteVALUTA()
        {
            if(!string.IsNullOrEmpty(_sie.VALUTA))
            {
                WriteLine("#VALUTA " + _sie.VALUTA);
            }
        }

        private void WriteVER()
        {
            foreach(var v in _sie.VER)
            {
                var createdBy = v.CreatedBy == "" ? "" : " \"" + v.CreatedBy + "\"";
                var createdDate = v.CreatedDate == 0 ? "" : v.CreatedDate.ToString();
                WriteLine("#VER \"" + v.Series + "\" \"" + v.Number + "\" " + makeSieDate(v.VoucherDate) + " \"" + v.Text + "\" " + createdDate + createdBy);

                WriteLine("{");
                
                foreach(var r in v.Rows)
                {
                    var obj = getObjeklista(r.Objects);
                    var quantity = r.Quantity.HasValue ? SieAmount(r.Quantity.Value) : "";
                    createdBy = v.CreatedBy == "" ? "" : "\"" + v.CreatedBy + "\"";
                    WriteLine("#TRANS " + r.Account.Number + " " + obj + " " + SieAmount(r.Amount) + " " + makeSieDate(r.RowDate) + " \"" + r.Text + "\" " + quantity + " " + createdBy);
                }
                
                WriteLine("}");
            }
        }

        private string getObjeklista(List<SieObject> objects)
        {
            if(_sie.SIETYP < 3) return "";

            var ret = "{";
            if (objects != null)
            {
                foreach (var o in objects)
                {
                    ret += o.Dimension.Number.ToString();
                    ret += " \"" + o.Number + "\" ";
                }
            }
            ret += "}";

            return ret;
        }

        private void WriteDIM()
        {
            foreach(var d in _sie.DIM.Values)
            {
                WriteLine("#DIM " + d.Number + " \"" + d.Name + "\"");

                foreach(var o in d.Objects.Values)
                {
                    WriteLine("#OBJEKT " + d.Number + " " + o.Number + " \"" + o.Name + "\"");
                }
            }
        }

        private void WritePeriodValue(string name, List<SiePeriodValue> list)
        {
            foreach(var v in list)
            {
                var objekt = getObjeklista(v.Objects);
                if ("#IB#UB#RES".Contains(name)) objekt = "";

                WriteLine(name + " " + v.YearNr.ToString() + " " + v.Account.Number + " " + objekt + " " + SieAmount(v.Amount));
            }
        }

        private void WritePeriodSaldo(string name, List<SiePeriodValue> list)
        {
            foreach (var v in list)
            {
                var objekt = getObjeklista(v.Objects);
                WriteLine(name + " " + v.YearNr.ToString() + " " + v.Period.ToString() + " " + v.Account.Number + " " + objekt + " " + SieAmount(v.Amount));
            }
        }

        private void WriteRAR()
        {
            foreach(var r in _sie.RAR.Values)
            {
                WriteLine("#RAR " + r.ID.ToString() + " " + SieDate(r.Start) + " " + SieDate(r.End));
            }
        }
        private string SieDate(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.Year.ToString() + date.Value.Month.ToString().PadLeft(2, '0') + date.Value.Day.ToString().PadLeft(2, '0');
            }
            else
            {
                return "";
            }
        }
        private string SieAmount(decimal amount)
        {
            return amount.ToString().Replace(',', '.');
        }

        private void WriteKONTO()
        {
            foreach(var k in _sie.KONTO.Values)
            {
                WriteLine("#KONTO " + k.Number + " \"" + k.Name + "\"");
                if (!string.IsNullOrWhiteSpace(k.Unit))
                {
                    WriteLine("#ENHET " + k.Number + " \"" + k.Unit + "\"");
                }
                if(!string.IsNullOrWhiteSpace(k.Type))
                {
                    WriteLine("#KTYP " + k.Number + " " + k.Type);
                }
            }
            foreach (var k in _sie.KONTO.Values)
            {
                foreach(var s in k.SRU)
                {
                    WriteLine("#SRU " + k.Number + " " + s);
                }
            }
        }

        private void WriteOMFATTN()
        {
            if(_sie.OMFATTN.HasValue)
            {
                WriteLine("#OMFATTN " + SieDate(_sie.OMFATTN));
            }
        }

        private void WriteTAXAR()
        {

            if (_sie.TAXAR > 0)
            {
                WriteLine("#TAXAR " + _sie.TAXAR.ToString()); 
            }
        }

        private void WriteFTYP()
        {
            if (!string.IsNullOrWhiteSpace(_sie.FNAMN.OrgType))
            {
                WriteLine("#FTYP " + _sie.FNAMN.OrgType);
            }
        }

        private void WriteKPTYP()
        {
            if (!string.IsNullOrWhiteSpace(_sie.KPTYP))
            {
                WriteLine("#KPTYP " + _sie.KPTYP);
            }
        }

        private void WriteADRESS()
        {
            if (!(_sie.FNAMN.Contact == null && _sie.FNAMN.Street == null && _sie.FNAMN.ZipCity == null && _sie.FNAMN.Phone == null))
            {
                WriteLine("#ADRESS \"" + _sie.FNAMN.Contact + "\" \"" + _sie.FNAMN.Street + "\" \"" + _sie.FNAMN.ZipCity + "\" \"" + _sie.FNAMN.Phone + "\"");
            }
        }
        private string FNAMN
        {
            get { return "#FNAMN \"" + _sie.FNAMN.Name + "\""; }
        }
        private string ORGNR
        {
            get { return "#ORGNR " + _sie.FNAMN.OrgIdentifier; }
        }

        private void WriteFNR()
        {
            if (!string.IsNullOrWhiteSpace(_sie.FNAMN.Code))
            WriteLine( "#FNR \"" + _sie.FNAMN.Code + "\"");
        }
        private string SIETYP
        {
            get { return "#SIETYP " + _sie.SIETYP.ToString(); }
        }
        
        private string GEN
        {
            get
            {
                string ret = "#GEN ";
                ret += makeSieDate(_sie.GEN_DATE) + " ";
                ret += makeField(_sie.GEN_NAMN);
                return ret;
            }
        }
        
        private string FORMAT
        {
            get { return "#FORMAT PC8"; }
        }
        private string PROGRAM
        {
            get
            {
                string program = "#PROGRAM ";
                foreach (var s in _sie.PROGRAM)
                {
                    program += "\"jsiSIE\" " +  makeField(s) + " ";
                }
                return program;
            }
        }
        private string FLAGGA
        {
            get
            {
                return "#FLAGGA " + _sie.FLAGGA.ToString();
            }
        }
        private void WriteLine(string line)
        {
            var bytes = _encoding.GetBytes(line.Trim() + Environment.NewLine);
            _stream.Write(bytes, 0, bytes.Length);
        }
        private string makeField(string data)
        {
            int foo;
            if (int.TryParse(data, out foo))
            {
                return data;
            }
            else
            {
                return "\"" + data + "\"";
            }
        }
        private string makeSieDate(DateTime? date)
        {
            if (date.HasValue)
            {
                return date.Value.ToString("yyyyMMdd");
            }
            else
            {
                return "00000000";
            }
        }
    }
}
