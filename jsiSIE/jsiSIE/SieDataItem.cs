using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace jsiSIE
{
    internal class SieDataItem
    {
        internal SieDocument Document { get; set; }

        internal string ItemType { get; set; }

        internal List<string> Data { get; set; }

        internal string RawData { get; set; }

        internal SieDataItem(string line, SieDocument document)
        {
            RawData = line;
            Document = document;
            var l = line.Trim();
            var p = FirstWhiteSpace(l);

            if (p == -1)
            {
                ItemType = l;
                Data = new List<string>();
            }
            else
            {
                ItemType = l.Substring(0, p);
                Data = splitLine(l.Substring(p + 1, l.Length - (p + 1)));
                
            }
        }

        private int FirstWhiteSpace(string str)
        {
            int a = str.IndexOf(" ");
            int b = str.IndexOf("\t");

            if (a == -1 && b == -1) return -1;
            if (a == -1 && b != -1) return b;
            if (b == -1) return a;

            if (a <= b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        private List<string> splitLine(string untrimmedData)
        {
            var data = untrimmedData.Trim();

            var ret = new List<string>();

            int isInField = 0;
            bool isInObject = false;
            string buffer = "";

            bool skipNext = false;
            foreach (char c in data)
            {
                if (skipNext && (c == '"'))
                {
                    skipNext = false;
                    continue;
                }

                if (c == '\\')
                {
                    skipNext = true;
                    continue;
                }

                if (c == '"' && !isInObject)
                {
                    isInField += 1;
                    continue;
                }

                if (c == '{') isInObject = true;
                if (c == '}') isInObject = false;

                if ((c == ' ' || c == '\t') && (isInField != 1) && !isInObject)
                {

                    var trimBuf = buffer.Trim();
                    if (trimBuf.Length > 0 || isInField == 2)
                    {
                        ret.Add(trimBuf);
                        buffer = "";
                    }
                    isInField = 0;
                }

                buffer += c;
            }
            if (buffer.Length > 0)
            {
                ret.Add(buffer.Trim());
            }

            return ret;
        }

        internal long GetLong(int field)
        {
            if (Data.Count <= field) return 0;

            long i = 0;
            long.TryParse(Data[field], out i);
            return i;
        }

        internal int GetInt(int field)
        {
            int? foo = GetIntNull(field);
            return foo.HasValue ? foo.Value : 0;
        }

        internal int? GetIntNull(int field)
        {
            if (Data.Count <= field) return null;

            int i = 0;
            int.TryParse(Data[field], out i);
            return i;
        }

        internal decimal GetDecimal(int field)
        {
            decimal? foo = GetDecimalNull(field);
            return foo.HasValue ? foo.Value : 0;
        }

        internal decimal? GetDecimalNull(int field)
        {
            if (Data.Count <= field) return null;

            decimal d = 0;
            var sep = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            var foo = Data[field].Replace(".", sep);
            decimal.TryParse(foo, out d);
            return d;
        }

        internal string GetString(int field)
        {
            if (Data.Count <= field) return "";

            string s = Data[field].Trim();
            s = s.Trim(new char[] { '"' });

            return s;
        }

        internal DateTime? GetDate(int field)
        {
            if (Data.Count <= field) return null;

            var fieldDate = Data[field].Trim();

            if (string.IsNullOrEmpty(fieldDate)) return null;

            if(fieldDate == "00000000") return null;

            var dateFormat = this.Document.DateFormat;
            DateTime parsedDateTime;
            
            if(!DateTime.TryParseExact(fieldDate,dateFormat, CultureInfo.InvariantCulture,DateTimeStyles.None, out parsedDateTime))
            {
                Document.Callbacks.CallbackException(new SieDateException($"{fieldDate} is not a valid date (raw data: '{this.RawData}', date format: {dateFormat})"));
                return null;
            }
            else
            {
                return parsedDateTime;
            }
        }

        internal List<SieObject> GetObjects()
        {
            var item = this;
            string dimNumber = null;
            string objectNumber = null;
            var ret = new List<SieObject>();


            if (item.RawData.Contains("{}")) return null;

            string data = null;
            foreach (var i in item.Data)
            {
                if (i.Trim().StartsWith("{"))
                {
                    data = i.Trim().Replace("{", "").Replace("}","");
                    break;
                }
            }

            if(data == null) 
            {
                item.Document.Callbacks.CallbackException(new SieMissingObjectException(item.RawData));
                return null;
            }

            var dimData = splitLine(data);

            for (int i = 0; i < dimData.Count(); i += 2)
            {
                dimNumber = dimData[i];

                var d = GetDimension(item, dimNumber);
                
                objectNumber = dimData[i + 1];

                //Add temporary object if the objects hasn't been loaded yet.
                if (!d.Objects.ContainsKey(objectNumber))
                {
                    d.Objects.Add(objectNumber, new SieObject() { Dimension = d, Number = objectNumber, Name = "[TEMP]" });
                }

                ret.Add(d.Objects[objectNumber]);
            }
            

            return ret;
        }

        internal SieDimension GetDimension(SieDataItem item, string dimNumber)
        {
            if (item.Document.UNDERDIM.ContainsKey(dimNumber))
            {
                return item.Document.UNDERDIM[dimNumber];
            }

            if (item.Document.DIM.ContainsKey(dimNumber))
            {
                return item.Document.DIM[dimNumber];
            }

            if (item.Document.TEMPDIM.ContainsKey(dimNumber))
            {
                return item.Document.TEMPDIM[dimNumber];
            }

            //Add temporary Dimension if the dimensions hasn't been loaded yet.
            item.Document.TEMPDIM.Add(dimNumber, new SieDimension() { Number = dimNumber, Name = "[TEMP]" });
            return item.Document.TEMPDIM[dimNumber];         
        }
    }
}