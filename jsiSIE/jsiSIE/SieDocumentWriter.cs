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
        
        private WriteOptions _options;
        

        public class WriteOptions
        {
            public WriteOptions(){
                this.Encoding = EncodingHelper.GetDefault();;
            }

            public bool WriteKSUMMA { get; set; } = false;
            public Encoding Encoding{ get; set; }
            public string DateFormat = "yyyyMMdd";
        }

        public SieDocumentWriter(SieDocument sie, WriteOptions options = null)
        {
            _sie = sie;
            _options = options ?? new WriteOptions();

        }


        public void Write(string file)
        {
            if (_options.WriteKSUMMA)
            {
                SetDocumentKSUMMA();
            }
            

            if (File.Exists(file)) File.Delete(file);
            using (_stream =  File.OpenWrite(file))
            {
                WriteCore();
            };
        }
 

        public void Write(Stream stream)
        {
            if (stream == null) throw new System.ArgumentNullException(nameof(stream));

            if (_options.WriteKSUMMA)
            {
                SetDocumentKSUMMA();
            }
            _stream = stream;
            WriteCore();
        }

        private void SetDocumentKSUMMA()
        {
            using (_stream = new MemoryStream())
            {
                WriteCore();
                _stream.Position = 0;
                var tempDoc = new SieDocument();
                tempDoc.ThrowErrors = false;
                tempDoc.Encoding = _options.Encoding;
                tempDoc.ReadDocument(_stream);
                _sie.KSUMMA = tempDoc.CRC.Checksum();
            }

            _stream = null;
        }

        private void WriteCore()
        {
            WriteLine(FLAGGA);

            if (_options.WriteKSUMMA) WriteLine("#KSUMMA");

            WriteLine(PROGRAM);
            WriteLine(FORMAT);
            WriteLine(GEN);
            WriteLine(SIETYP);
            if (!string.IsNullOrEmpty(_sie.PROSA)) WriteLine("#PROSA " + "\"" + _sie.PROSA + "\"");
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
            if (_options.WriteKSUMMA) WriteLine("#KSUMMA " + _sie.KSUMMA.ToString());
        }


        private void WriteVALUTA()
        {
            if (!string.IsNullOrEmpty(_sie.VALUTA))
            {
                WriteLine("#VALUTA " + _sie.VALUTA);
            }
        }

        private void WriteVER()
        {
            if (_sie.VER == null) return;
            foreach (var v in _sie.VER)
            {
                var createdBy = string.IsNullOrWhiteSpace(v.CreatedBy) ? "" : "\"" + v.CreatedBy + "\"";
                // Use an empty string rather than the default date of 000010101, when this optional field is not set
                var createdDate = v.CreatedDate == DateTime.MinValue ? "" : makeSieDate(v.CreatedDate); 
                WriteLine("#VER \"" + v.Series + "\" \"" + v.Number + "\" " + makeSieDate(v.VoucherDate) + " \"" + v.Text + "\" " + createdDate + " " + createdBy);

                WriteLine("{");

                foreach (var r in v.Rows)
                {
                    var obj = getObjeklista(r.Objects);
                    var quantity = r.Quantity.HasValue ? SieAmount(r.Quantity.Value) : "";
                    createdBy = string.IsNullOrWhiteSpace(r.CreatedBy) ? "" : "\"" + r.CreatedBy + "\"";
                    WriteLine(r.Token +  " " + r.Account.Number + " " + obj + " " + SieAmount(r.Amount) + " " + makeSieDate(r.RowDate) + " \"" + r.Text + "\" " + quantity + " " + createdBy);
                }

                WriteLine("}");
            }
        }

        private string getObjeklista(List<SieObject> objects)
        {
            if (_sie.SIETYP < 3) return "";

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
            if (_sie.DIM == null) return;
            foreach (var d in _sie.DIM.Values)
            {
                WriteLine("#DIM " + d.Number + " \"" + d.Name + "\"");

                foreach (var o in d.Objects.Values)
                {
                    WriteLine("#OBJEKT " + d.Number + " " + o.Number + " \"" + o.Name + "\"");
                }
            }
        }

        private void WritePeriodValue(string name, List<SiePeriodValue> list)
        {
            if (list == null) return;
            foreach (var v in list)
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
            if (_sie.RAR == null) return;
            foreach (var r in _sie.RAR.Values)
            {
                WriteLine("#RAR " + r.ID.ToString() + " " + makeSieDate(r.Start) + " " + makeSieDate(r.End));
            }
        }
        private string SieAmount(decimal amount)
        {
            return amount.ToString().Replace(',', '.');
        }

        private void WriteKONTO()
        {
            if (_sie.KONTO == null) return;
            foreach (var k in _sie.KONTO.Values)
            {
                WriteLine("#KONTO " + k.Number + " \"" + k.Name + "\"");
                if (!string.IsNullOrWhiteSpace(k.Unit))
                {
                    WriteLine("#ENHET " + k.Number + " \"" + k.Unit + "\"");
                }
                if (!string.IsNullOrWhiteSpace(k.Type))
                {
                    WriteLine("#KTYP " + k.Number + " " + k.Type);
                }
            }
            foreach (var k in _sie.KONTO.Values)
            {
                foreach (var s in k.SRU)
                {
                    WriteLine("#SRU " + k.Number + " " + s);
                }
            }
        }

        private void WriteOMFATTN()
        {
            if (_sie.OMFATTN.HasValue)
            {
                WriteLine("#OMFATTN " + makeSieDate(_sie.OMFATTN));
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
                WriteLine("#FNR \"" + _sie.FNAMN.Code + "\"");
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
            get { 
                if(_options.Encoding.CodePage == 437){
                    return "#FORMAT PC8"; 
                } else{
                    return $"#FORMAT {_options.Encoding.EncodingName}";
                }
                
                }
        }
        private string PROGRAM
        {
            get
            {
                string program = "#PROGRAM \"jsiSIE\" ";
                foreach (var s in _sie.PROGRAM)
                {
                    program += makeField(s) + " ";
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
            var bytes = _options.Encoding.GetBytes(line.Trim() + Environment.NewLine);
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
                return date.Value.ToString(_options.DateFormat);
            }
            else
            {
                if(_options.DateFormat == "yyyyMMdd"){
                    return "00000000";
                } else{
                    return DateTime.MinValue.ToString(_options.DateFormat);
                }
            }
        }
    }
}