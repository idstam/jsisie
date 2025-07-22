using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    [Serializable]
    public class SieDocument
    {
        /// <summary>
        /// This is where all the callbacks to client code happens.
        /// </summary>
        public SieCallbacks Callbacks = new SieCallbacks();

        public bool IgnoreBTRANS = false;
        public bool IgnoreMissingOMFATTNING = false;
        public bool IgnoreRTRANS = false;
        public bool IgnoreMissingDate = true;
        public bool AllowMissingDate { get => IgnoreMissingDate; set => IgnoreMissingDate = value; }
        public bool AllowUnbalancedVoucher { get;  set; }

        public bool AllowUnderDimensions { get; set; }

        public string DateFormat = "yyyyMMdd";
        public Encoding Encoding;

        /// <summary>
        /// If this is set to true in ReadFile no period values, balances or transactions will be saved in memory.
        /// Use this in combination with callbacks to stream through a file.
        /// </summary>
        public bool StreamValues = false;

        /// <summary>
        /// Calculates KSUMMA
        /// </summary>
        internal SieCRC32 CRC;

        [Flags]
        public enum SieType
        {
            SIE1 = 1,
            SIE2 = 2,
            SIE3 = 4,
            SIE4 = 8
        }

        private static readonly Dictionary<int, SieType> SieTypeMap = new Dictionary<int, SieType>()
        {
            { 1, SieType.SIE1 },
            { 2, SieType.SIE2 },
            { 3, SieType.SIE3 },
            { 4, SieType.SIE4 }
        };

        /// <summary>
        /// This is the SIE types that will be accepted when reading a file. Reading will stop if a file with a different SIE type is encountered.
        /// As default all SIE types are accepted (SIE1 | SIE2 | SIE3 | SIE4).
        /// For example, if you only want to accept SIE2 and SIE3 files, set AcceptSIETypes = SieType.SIE2 | SieType.SIE3.
        /// (This is a alternative to method GetSieVersion which will return the SIE type of a file but is done in a separate read.)
        /// </summary>
        public SieType AcceptSIETypes { get; set; } = SieType.SIE1 | SieType.SIE2 | SieType.SIE3 | SieType.SIE4;

        /// <summary>
        /// This is the file currently being read.
        /// </summary>
        private string _fileName;
        public SieDocument()
        {
            this.Encoding = EncodingHelper.GetDefault();
        }


        /// <summary>
        /// #DIM
        /// </summary>
        public Dictionary<string, SieDimension> DIM { get; set; }

        /// <summary>
        /// #UNDERDIM
        /// </summary>
        public Dictionary<string, SieDimension> UNDERDIM { get; set; }

        /// <summary>
        /// #DIM or #UNDERDIM, should be empty after reading file.
        /// </summary>
        internal Dictionary<string, SieDimension> TEMPDIM { get; set; }

        /// <summary>
        /// #FLAGGA
        /// </summary>
        public int FLAGGA { get; set; }

        public SieCompany FNAMN { get; set; }

        /// <summary>
        /// #FORMAT
        /// </summary>
        public string FORMAT { get; set; }

        /// <summary>
        /// #GEN
        /// </summary>
        public DateTime? GEN_DATE { get; set; }

        public string GEN_NAMN { get; set; }

        /// <summary>
        /// #IB
        /// </summary>
        public List<SiePeriodValue> IB { get; set; }

        /// <summary>
        /// #KONTO
        /// </summary>
        public Dictionary<string, SieAccount> KONTO { get; set; }

        /// <summary>
        /// #KPTYP
        /// </summary>
        public string KPTYP { get; set; }

        public long KSUMMA { get; set; }

        /// <summary>
        /// #OIB
        /// </summary>
        public List<SiePeriodValue> OIB { get; set; }

        /// <summary>
        /// #OMFATTN Obligatory when exporting period values
        /// </summary>
        public DateTime? OMFATTN { get; set; }

        /// <summary>
        /// #OUB
        /// </summary>
        public List<SiePeriodValue> OUB { get; set; }

        /// <summary>
        /// #PBUDGET
        /// </summary>
        public List<SiePeriodValue> PBUDGET { get; set; }

        /// <summary>
        /// #PROGRAM
        /// </summary>
        public List<string> PROGRAM { get; set; }

        /// <summary>
        /// #PROSA
        /// </summary>
        public string PROSA { get; set; }

        /// <summary>
        /// #PSALDO
        /// </summary>
        public List<SiePeriodValue> PSALDO { get; set; }

        /// <summary>
        /// #RAR
        /// </summary>
        public Dictionary<int, SieBookingYear> RAR { get; set; }

        /// <summary>
        /// #RES
        /// </summary>
        public List<SiePeriodValue> RES { get; set; }

        /// <summary>
        /// #SIETYP
        /// </summary>
        public int SIETYP { get; set; }

        /// <summary>
        /// #TAXAR
        /// </summary>
        public int TAXAR { get; set; }

        /// <summary>
        /// If this is set to true in ReadFile each error will be thrown otherwise they will just be callbacked.
        /// </summary>
        public bool ThrowErrors = true;
        /// <summary>
        /// #UB
        /// </summary>
        public List<SiePeriodValue> UB { get; set; }

        /// <summary>
        /// Will contain all validation errors after doing a ValidateDocument
        /// </summary>
        public List<Exception> ValidationExceptions { get; set; }
        /// <summary>
        /// #VALUTA
        /// </summary>
        public string VALUTA { get; set; }

        /// <summary>
        /// #VER
        /// </summary>
        public List<SieVoucher> VER { get; set; }


        /// <summary>
        /// Does a fast scan of the file to get the Sie version it adheres to.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>-1 if no SIE version was found in the file else SIETYPE is returned.</returns>
        public static int GetSieVersion(string fileName)
        {
            return GetSieVersion(fileName, EncodingHelper.GetDefault());
        }
        public static int GetSieVersion(string fileName, Encoding encoding)
        {
            int ret = -1;
            foreach (var line in File.ReadLines(fileName, encoding))
            {
                if (line.StartsWith("#SIETYP"))
                {
                    var di = new SieDataItem(line, null);
                    ret = di.GetInt(0);
                    break;
                }
            }

            return ret;
        }

        public void ReadDocument(string fileName)
        {
            _fileName = fileName;

            using (var stream = new FileStream(_fileName, FileMode.Open))
            {
                readStreamAux(stream);
            }
        }

        public async Task ReadDocumentAsync(string fileName)
        {
            _fileName = fileName;

            using (var stream = new FileStream(_fileName, FileMode.Open))
            {
                await readStreamAuxAsync(stream);
            }
        }

        public void ReadDocument(Stream stream)
        {
            _fileName = "(stream)";
            readStreamAux(stream);
        }
        
        public async Task ReadDocumentAsync(Stream stream)
        {
            _fileName = "(stream)";
            await readStreamAuxAsync(stream);
        }
        
        private void readStreamAux(Stream stream)
        {
            initialize();

            using (var sr = new StreamReader(stream, this.Encoding))
            {
                if (parseLines(sr)) return;
            }

            validateDocument();
        }
        
        private async Task readStreamAuxAsync(Stream stream)
        {
            initialize();

            using (var sr = new StreamReader(stream, this.Encoding))
            {
                if (await parseLinesAsync(sr)) return;
            }

            validateDocument();
        }

        private void initialize() {
              if (ThrowErrors) Callbacks.SieException += throwCallbackException;
  
              #region Initialize lists
              FNAMN = new SieCompany();
  
              KONTO = new Dictionary<string, SieAccount>();
              DIM = new Dictionary<string, SieDimension>();
              UNDERDIM = new Dictionary<string, SieDimension>();
              TEMPDIM = new Dictionary<string, SieDimension>();
  
              OIB = new List<SiePeriodValue>();
              OUB = new List<SiePeriodValue>();
              PSALDO = new List<SiePeriodValue>();
              PBUDGET = new List<SiePeriodValue>();
              PROGRAM = new List<string>();
              RAR = new Dictionary<int, SieBookingYear>();
              IB = new List<SiePeriodValue>();
              UB = new List<SiePeriodValue>();
              RES = new List<SiePeriodValue>();
  
              VER = new List<SieVoucher>();
              ValidationExceptions = new List<Exception>();
  
              initializeDimensions();
              #endregion //Initialize listst
  
              CRC = new SieCRC32(this.Encoding);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <returns>true if start of file is valid SIE-format</returns>
        private bool parseLines(StreamReader sr)
        {
            bool firstLine = true;
            SieVoucher curVoucher = null;
            do
            {
                var line = sr.ReadLine();

                if (parseLine(line, ref curVoucher, ref firstLine)) return true;
            } while (!sr.EndOfStream);

            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sr"></param>
        /// <returns>true if start of file is valid SIE-format</returns>
        private async Task<bool> parseLinesAsync(StreamReader sr) 
        {
            bool firstLine = true;
            SieVoucher curVoucher = null;
            do {
                var line = await sr.ReadLineAsync();

                if (parseLine(line, ref curVoucher, ref firstLine)) return true;
            } while (!sr.EndOfStream);

            return false;
        }

        private bool parseLine(string line, ref SieVoucher curVoucher, ref bool firstLine) {
                Callbacks.CallbackLine(line);
                var di = new SieDataItem(line, this);

                if (firstLine)
                {
                    firstLine = false;
                    if (di.ItemType != "#FLAGGA")
                    {
                        Callbacks.CallbackException(new SieInvalidFileException(_fileName));
                        return true;
                    }
                }

                if (CRC.Started && di.ItemType != "#KSUMMA") CRC.AddData(di);

                SiePeriodValue pv = null;

                switch (di.ItemType)
                {
                    case "#ADRESS":
                        FNAMN.Contact = di.GetString(0);
                        FNAMN.Street = di.GetString(1);
                        FNAMN.ZipCity = di.GetString(2);
                        FNAMN.Phone = di.GetString(3);
                        break;

                    case "#BKOD":
                        FNAMN.SNI = di.GetInt(0);
                        break;

                    case "#BTRANS":
                        if (!IgnoreBTRANS) parseTRANS(di, curVoucher);
                        break;

                    case "#DIM":
                        parseDimension(di);
                        break;

                    case "#UNDERDIM":
                        parseUnderDimension(di);
                        break;

                    case "#ENHET":
                        parseENHET(di);
                        break;

                    case "#FLAGGA":
                        FLAGGA = di.GetInt(0);
                        break;

                    case "#FNAMN":
                        FNAMN.Name = di.GetString(0);
                        break;

                    case "#FNR":
                        FNAMN.Code = di.GetString(0);
                        break;

                    case "#FORMAT":
                        FORMAT = di.GetString(0);
                        break;

                    case "#FTYP":
                        FNAMN.OrgType = di.GetString(0);
                        break;

                    case "#GEN":
                        GEN_DATE = di.GetDate(0);
                        GEN_NAMN = di.GetString(1);
                        break;

                    case "#IB":
                        parseIB(di);
                        break;

                    case "#KONTO":
                        parseKONTO(di);
                        break;
                    case "#KSUMMA":
                        if (CRC.Started)
                        {
                            parseKSUMMA(di);
                        }
                        else
                        {
                            CRC.Start();
                        }

                        break;
                    case "#KTYP":
                        parseKTYP(di);
                        break;

                    case "#KPTYP":
                        KPTYP = di.GetString(0);
                        break;

                    case "#OBJEKT":
                        parseOBJEKT(di);
                        break;

                    case "#OIB":
                        pv = parseOIB_OUB(di);
                        Callbacks.CallbackOIB(pv);
                        if (!StreamValues) OIB.Add(pv);
                        break;

                    case "#OUB":
                        pv = parseOIB_OUB(di);
                        Callbacks.CallbackOUB(pv);
                        if (!StreamValues) OUB.Add(pv);
                        break;

                    case "#ORGNR":
                        FNAMN.OrgIdentifier = di.GetString(0);
                        break;

                    case "#OMFATTN":
                        OMFATTN = di.GetDate(0);
                        break;

                    case "#PBUDGET":
                        pv = parsePBUDGET_PSALDO(di);
                        if (pv != null)
                        {
                            Callbacks.CallbackPBUDGET(pv);
                            if (!StreamValues) PBUDGET.Add(pv);
                        }

                        break;

                    case "#PROGRAM":
                        PROGRAM = di.Data;
                        break;

                    case "#PROSA":
                        PROSA = di.GetString(0);
                        break;

                    case "#PSALDO":
                        pv = parsePBUDGET_PSALDO(di);
                        if (pv != null)
                        {
                            Callbacks.CallbackPSALDO(pv);
                            if (!StreamValues) PSALDO.Add(pv);
                        }

                        break;

                    case "#RAR":
                        parseRAR(di);
                        break;

                    case "#RTRANS":
                        if (!IgnoreBTRANS) parseTRANS(di, curVoucher);
                        break;

                    case "#SIETYP":
                        SIETYP = di.GetInt(0);
                        
                        if (!AcceptSIETypes.HasFlag(SieTypeMap[SIETYP]))
                        {
                            Callbacks.CallbackException(new SieInvalidFileException($"File of SIE type {SIETYP} is not accepted in current implementation."));
                            return true;
                        }
                        
                        break;

                    case "#SRU":
                        parseSRU(di);
                        break;

                    case "#TAXAR":
                        TAXAR = di.GetInt(0);
                        break;

                    case "#UB":
                        parseUB(di);
                        break;

                    case "#TRANS":
                        parseTRANS(di, curVoucher);
                        break;
                    case "#RES":
                        parseRES(di);
                        break;

                    case "#VALUTA":
                        VALUTA = di.GetString(0);
                        break;

                    case "#VER":
                        curVoucher = parseVER(di);
                        break;

                    case "":
                        //Empty line
                        break;
                    case "{":
                        break;
                    case "}":
                        if (curVoucher != null) closeVoucher(curVoucher);
                        curVoucher = null;
                        break;
                    default:
                        Callbacks.CallbackException(new NotImplementedException(di.ItemType));
                        break;
                }

            return false;
        }


        private void parseRAR(SieDataItem di)
        {

            rar = new SieBookingYear();
            rar.ID = di.GetInt(0);
            rar.Start = di.GetDate(1);
            rar.End = di.GetDate(2);

            RAR.Add(rar.ID, rar);
        }

        private void addValidationException(bool isException, Exception ex)
        {
            if (isException)
            {
                ValidationExceptions.Add(ex);
                Callbacks.CallbackException(ex);
            }
        }

        private void closeVoucher(SieVoucher v)
        {
            //Check sum of rows
            decimal check = 0;
            foreach (var r in v.Rows)
            {
                if (r.Token == "#RTRANS" && this.IgnoreRTRANS) continue;
                if (r.Token == "#BTRANS" && this.IgnoreBTRANS) continue;

                check += r.Amount;
            }
            if (check != 0 && !this.AllowUnbalancedVoucher)
            {
                var ex = new SieVoucherMissmatchException(v.Series + "." + v.Number + " Sum is not zero.");
                ValidationExceptions.Add(ex);
                Callbacks.CallbackException(ex);
            }

            Callbacks.CallbackVER(v);
            if (!StreamValues) VER.Add(v);

        }

        private void initializeDimensions()
        {
            DIM.Add("1", new SieDimension() { Number = "1", Name = "Resultatenhet", IsDefault = true });
            if(AllowUnderDimensions)
                UNDERDIM.Add("2", new SieDimension() { Number = "2", Name = "Kostnadsbärare", SuperDim = DIM["1"], IsDefault = true });
            else
                DIM.Add("2", new SieDimension() { Number = "2", Name = "Kostnadsbärare", SuperDim = DIM["1"], IsDefault = true });
            DIM.Add("3", new SieDimension() { Number = "3", Name = "Reserverat", IsDefault = true });
            DIM.Add("4", new SieDimension() { Number = "4", Name = "Reserverat", IsDefault = true });
            DIM.Add("5", new SieDimension() { Number = "5", Name = "Reserverat", IsDefault = true });
            DIM.Add("6", new SieDimension() { Number = "6", Name = "Projekt", IsDefault = true });
            DIM.Add("7", new SieDimension() { Number = "7", Name = "Anställd", IsDefault = true });
            DIM.Add("8", new SieDimension() { Number = "8", Name = "Kund", IsDefault = true });
            DIM.Add("9", new SieDimension() { Number = "9", Name = "Leverantör", IsDefault = true });
            DIM.Add("10", new SieDimension() { Number = "10", Name = "Faktura", IsDefault = true });
            DIM.Add("11", new SieDimension() { Number = "11", Name = "Reserverat", IsDefault = true });
            DIM.Add("12", new SieDimension() { Number = "12", Name = "Reserverat", IsDefault = true });
            DIM.Add("13", new SieDimension() { Number = "13", Name = "Reserverat", IsDefault = true });
            DIM.Add("14", new SieDimension() { Number = "14", Name = "Reserverat", IsDefault = true });
            DIM.Add("15", new SieDimension() { Number = "15", Name = "Reserverat", IsDefault = true });
            DIM.Add("16", new SieDimension() { Number = "16", Name = "Reserverat", IsDefault = true });
            DIM.Add("17", new SieDimension() { Number = "17", Name = "Reserverat", IsDefault = true });
            DIM.Add("18", new SieDimension() { Number = "18", Name = "Reserverat", IsDefault = true });
            DIM.Add("19", new SieDimension() { Number = "19", Name = "Reserverat", IsDefault = true });
        }

        private void parseDimension(SieDataItem di)
        {
            var d = di.GetString(0);
            var n = di.GetString(1);

            if (TEMPDIM.ContainsKey(d))
            {
                DIM.Add(d, TEMPDIM[d]);
                TEMPDIM.Remove(d);
            }

            if (!DIM.ContainsKey(d))
            {
                DIM.Add(d, new SieDimension() { Number = d });
            }

            DIM[d].Name = n;
            DIM[d].IsDefault = false;
        }

        private void parseUnderDimension(SieDataItem di)
        {
            if(!AllowUnderDimensions) return;

            var d = di.GetString(0);
            var n = di.GetString(1);
            var p = di.GetString(2);

            if(TEMPDIM.ContainsKey(d))
            {
                UNDERDIM.Add(d, TEMPDIM[d]);
                TEMPDIM.Remove(d);
            }
            
            if (!UNDERDIM.ContainsKey(d))
            {
                UNDERDIM.Add(d, new SieDimension() { Number = d });
            }

            UNDERDIM[d].Name = n;
            UNDERDIM[d].IsDefault = false;
            UNDERDIM[d].SuperDim = DIM[p];            
        }

        private void parseENHET(SieDataItem di)
        {
            if (!KONTO.ContainsKey(di.GetString(0)))
            {
                KONTO.Add(di.GetString(0), new SieAccount() { Number = di.GetString(0) });
            }
            KONTO[di.GetString(0)].Unit = di.GetString(1);
        }

        private void parseIB(SieDataItem di)
        {
            if (!KONTO.ContainsKey(di.GetString(1)))
            {
                KONTO.Add(di.GetString(1), new SieAccount() { Number = di.GetString(1) });
            }

            var v = new SiePeriodValue()
            {
                YearNr = di.GetInt(0),
                Account = KONTO[di.GetString(1)],
                Amount = di.GetDecimal(2),
                Quantity = di.GetDecimal(3),
                Token = di.ItemType
            };
            Callbacks.CallbackIB(v);
            if (!StreamValues) IB.Add(v);
        }

        private void parseKONTO(SieDataItem di)
        {
            if (KONTO.ContainsKey(di.GetString(0)))
            {
                KONTO[di.GetString(0)].Name = di.GetString(1);
            }
            else
            {
                KONTO.Add(di.GetString(0), new SieAccount() { Number = di.GetString(0), Name = di.GetString(1) });
            }
        }

        private void parseKSUMMA(SieDataItem di)
        {
            KSUMMA = di.GetLong(0);
            long checksum = CRC.Checksum();
            if (KSUMMA != checksum)
            {
                Callbacks.CallbackException(new SieInvalidChecksumException(_fileName));
            }

        }
        private void parseKTYP(SieDataItem di)
        {
            //Create the account if it hasn't been loaded yet.
            if (!KONTO.ContainsKey(di.GetString(0)))
            {
                KONTO.Add(di.GetString(0), new SieAccount() { Number = di.GetString(0) });
            }
            KONTO[di.GetString(0)].Type = di.GetString(1);
        }

        private void parseOBJEKT(SieDataItem di)
        {
            var dimNumber = di.GetString(0);
            var number = di.GetString(1);
            var name = di.GetString(2);

            var dim = di.GetDimension(di, dimNumber);

            var obj = new SieObject() { Dimension = dim, Number = number, Name = name };

            if (!dim.Objects.ContainsKey(number))
            {
                dim.Objects.Add(number, obj);
            }
            else
            {
                dim.Objects[number] = obj;
            }
        }

        private SiePeriodValue parseOIB_OUB(SieDataItem di)
        {
            //Create the account if it hasn't been loaded yet.
            if (!KONTO.ContainsKey(di.GetString(1)))
            {
                KONTO.Add(di.GetString(1), new SieAccount() { Number = di.GetString(1) });
            }

            if (SIETYP < 3)
            {
                Callbacks.CallbackException(new SieInvalidFeatureException("Neither OIB or OUB is part of SIE < 3"));
            }

            var objOffset = 0;
            if (di.RawData.Contains("{")) objOffset = 1;

            var v = new SiePeriodValue()
            {
                YearNr = di.GetInt(0),
                //Period = di.GetInt(1),
                Account = KONTO[di.GetString(1)],
                Amount = di.GetDecimal(2 + objOffset),
                Quantity = di.GetDecimal(3 + objOffset),
                Objects = di.GetObjects(),
                Token = di.ItemType
            };

            return v;
        }

        private SiePeriodValue parsePBUDGET_PSALDO(SieDataItem di)
        {
            //Create the account if it hasn't been loaded yet.
            if (!KONTO.ContainsKey(di.GetString(2)))
            {
                KONTO.Add(di.GetString(2), new SieAccount() { Number = di.GetString(2) });
            }

            if (SIETYP == 1)
            {
                Callbacks.CallbackException(new SieInvalidFeatureException("Neither PSALDO or PBUDGET is part of SIE 1"));
            }

            if (SIETYP == 2 && di.RawData.Contains("{") && !di.RawData.Contains("{}"))
            {
                //Applications reading SIE type 2 should ignore PSALDO containing non empty dimension.
                return null;
            }

            var objOffset = 0;
            if (di.RawData.Contains("{")) objOffset = 1;

            var v = new SiePeriodValue()
            {
                YearNr = di.GetInt(0),
                Period = di.GetInt(1),
                Account = KONTO[di.GetString(2)],
                Amount = di.GetDecimal(3 + objOffset),
                Quantity = di.GetDecimal(4 + objOffset),
                Token = di.ItemType
            };
            if (SIETYP != 2 && di.RawData.Contains("{")) v.Objects = di.GetObjects();
            return v;
        }

        private void parseRES(SieDataItem di)
        {
            if (!KONTO.ContainsKey(di.GetString(1)))
            {
                KONTO.Add(di.GetString(1), new SieAccount() { Number = di.GetString(1) });
            }
            var objOffset = 0;
            if (di.RawData.Contains("{")) objOffset = 1;
            var v = new SiePeriodValue()
            {
                YearNr = di.GetInt(0),
                Account = KONTO[di.GetString(1)],
                Amount = di.GetDecimal(2 + objOffset),
                Quantity = di.GetDecimal(3 + objOffset),
                Token = di.ItemType
            };
            Callbacks.CallbackRES(v);
            if (!StreamValues) RES.Add(v);
            return;
        }

        private void parseSRU(SieDataItem di)
        {
            if (!KONTO.ContainsKey(di.GetString(0)))
            {
                KONTO.Add(di.GetString(0), new SieAccount() { Number = di.GetString(0) });
            }
            KONTO[di.GetString(0)].SRU.Add(di.GetString(1));
        }

        private void parseTRANS(SieDataItem di, SieVoucher v)
        {
            if (!KONTO.ContainsKey(di.GetString(0)))
            {
                KONTO.Add(di.GetString(0), new SieAccount() { Number = di.GetString(0) });
            }

            var objOffset = 0;
            if (di.RawData.Contains("{")) objOffset = 1;

            var vr = new SieVoucherRow()
            {
                Account = KONTO[di.GetString(0)],
                Objects = di.GetObjects(),
                Amount = di.GetDecimal(1 + objOffset),
                RowDate = di.GetDate(2 + objOffset).HasValue ? di.GetDate(2 + objOffset).Value : v.VoucherDate,
                Text = di.GetString(3 + objOffset),
                Quantity = di.GetDecimalNull(4 + objOffset),
                CreatedBy = di.GetString(5 + objOffset),
                Token = di.ItemType
            };

            v.Rows.Add(vr);
        }

        private void parseUB(SieDataItem di)
        {
            if (!KONTO.ContainsKey(di.GetString(1)))
            {
                KONTO.Add(di.GetString(1), new SieAccount() { Number = di.GetString(1) });
            }
            var v = new SiePeriodValue()
            {
                YearNr = di.GetInt(0),
                Account = KONTO[di.GetString(1)],
                Amount = di.GetDecimal(2),
                Quantity = di.GetDecimal(3),
                Token = di.ItemType
            };
            Callbacks.CallbackUB(v);
            if (!StreamValues) UB.Add(v);

        }

        private SieVoucher parseVER(SieDataItem di)
        {
            if (!di.GetDate(2).HasValue) Callbacks.CallbackException(new MissingFieldException("Voucher date"));

            var v = new SieVoucher()
            {
                Series = di.GetString(0),
                Number = di.GetString(1),
                VoucherDate = di.GetDate(2).HasValue ? di.GetDate(2).Value : new DateTime(),
                Text = di.GetString(3),
                CreatedDate = di.GetDate(4).HasValue ? di.GetDate(4).Value : new DateTime(),
                CreatedBy = di.GetString(5),
                Token = di.ItemType
            };

            return v;
        }

        /// <summary>
        /// This is used to throw errors when throwError == true
        /// </summary>
        /// <param name="ex"></param>
        private void throwCallbackException(Exception ex)
        {
            throw ex;
        }

        private void validateDocument()
        {

            addValidationException((
                !IgnoreMissingDate &&
                !GEN_DATE.HasValue),
                new SieMissingMandatoryDateException("#GEN Date is missing in " + _fileName));

            //If there are period values #OMFATTN has to tell the value date.
            addValidationException(
                (!IgnoreMissingOMFATTNING) &&
                ((SIETYP == 2 || SIETYP == 3) &&
                !OMFATTN.HasValue &&
                (RES.Count > 0 || UB.Count > 0 || OUB.Count > 0)),
                new SieMissingMandatoryDateException("#OMFATTN is missing in " + _fileName));

            //Ignore KSUMMA for multi byte code pages.
            if(this.Encoding.IsSingleByte)
            {
                addValidationException(
                    (CRC.Started) &&
                    (KSUMMA == 0),
                    new SieInvalidChecksumException(_fileName));
            }

            // All TEMPDIMs should have been resolved when read is completed.
            addValidationException(TEMPDIM.Any(),
                new SieMissingMandatoryDateException("#DIM or #UNDERDIM is missing for one or more objects in " + _fileName));

        }

        public SieBookingYear rar { get; set; }
        
    }
}