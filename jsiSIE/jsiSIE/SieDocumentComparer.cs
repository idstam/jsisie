using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    public class SieDocumentComparer
    {
        private SieDocument _docA;
        private SieDocument _docB;
        private List<string> _errors;

        private SieDocumentComparer(SieDocument docA, SieDocument docB)
        {
            _docA = docA;
            _docB = docB;
            _errors = new List<string>();
        }


        public static List<string> Compare(SieDocument docA, SieDocument docB)
        {
            var comp = new SieDocumentComparer(docA, docB);
            comp.doCompare();
            return comp._errors;
        }
        private void doCompare()
        {
            compareNonListItems();
            compareProgram(_docA.PROGRAM, _docB.PROGRAM);
            compareDIM(_docA, _docB, "First", "Second");
            compareDIM(_docB, _docA, "Second", "First");

            comparePeriodValueList(_docA.IB, _docB.IB, "IB", "First", "Second");
            comparePeriodValueList(_docB.IB, _docA.IB, "IB", "Second", "First");
            comparePeriodValueList(_docA.UB, _docB.UB, "UB", "First", "Second");
            comparePeriodValueList(_docB.UB, _docA.UB, "UB", "Second", "First");
            comparePeriodValueList(_docA.OIB, _docB.OIB, "OIB", "First", "Second");
            comparePeriodValueList(_docB.OIB, _docA.OIB, "OIB", "Second", "First");
            comparePeriodValueList(_docA.OUB, _docB.OUB, "OUB", "First", "Second");
            comparePeriodValueList(_docB.OUB, _docA.OUB, "OUB", "Second", "First");
            comparePeriodValueList(_docA.PBUDGET, _docB.PBUDGET, "PBUDGET", "First", "Second");
            comparePeriodValueList(_docB.PBUDGET, _docA.PBUDGET, "PBUDGET", "Second", "First");
            comparePeriodValueList(_docA.PSALDO, _docB.PSALDO, "PSALDO", "First", "Second");
            comparePeriodValueList(_docB.PSALDO, _docA.PSALDO, "PSALDO", "Second", "First");
            comparePeriodValueList(_docA.RES, _docB.RES, "RES", "First", "Second");
            comparePeriodValueList(_docB.RES, _docA.RES, "RES", "Second", "First");

            compareKONTO(_docA, _docB, "First", "Second");
            compareKONTO(_docB, _docA, "Second", "First");

            compareRAR(_docA, _docB, "First", "Second");
            compareRAR(_docB, _docA, "Second", "First");

            compareVER(_docA, _docB, "First", "Second");
            compareVER(_docB, _docA, "Second", "First");
            
        }
        private void compareNonListItems()
        {
            if (_docA.FLAGGA != _docB.FLAGGA) _errors.Add("FLAGGA differs First, Second " + _docA.FLAGGA + " , " + _docB.FLAGGA);

            if (_docA.FORMAT != _docB.FORMAT) _errors.Add("FORMAT differs First, Second " + _docA.FORMAT + " , " + _docB.FORMAT);

            compareFNAMN();

            if (_docA.FORMAT != _docB.FORMAT) _errors.Add("FORMAT differs First, Second " + _docA.FORMAT + " , " + _docB.FORMAT);

            if (!_docA.GEN_DATE.HasValue && _docB.GEN_DATE.HasValue && _docA.GEN_DATE.Value == _docB.GEN_DATE.Value)
            {
                _errors.Add("GEN_DATE differs");
            }
            if (!_docA.OMFATTN.HasValue && _docB.OMFATTN.HasValue && _docA.OMFATTN.Value == _docB.OMFATTN.Value)
            {
                _errors.Add("OMFATTN differs");
            }
            if (!strEqual(_docA.GEN_NAMN, _docB.GEN_NAMN)) _errors.Add("GEN_NAMN differs First, Second " + _docA.GEN_NAMN + " , " + _docB.GEN_NAMN);
            if (!strEqual(_docA.KPTYP, _docB.KPTYP)) _errors.Add("KPTYP differs First, Second " + _docA.KPTYP + " , " + _docB.KPTYP);
            //if (_docA.KSUMMA != _docB.KSUMMA) _errors.Add("KSUMMA differs First, Second " + _docA.KSUMMA + " , " + _docB.KSUMMA);
            var a = _docA.PROSA ?? "";
            var b = _docB.PROSA ?? "";
            if (a != b) _errors.Add("PROSA differs First, Second " + a + " , " + b);
            if (_docA.SIETYP != _docB.SIETYP) _errors.Add("SIETYP differs First, Second " + _docA.SIETYP + " , " + _docB.SIETYP);
            if (_docA.TAXAR != _docB.TAXAR) _errors.Add("TAXAR differs First, Second " + _docA.TAXAR + " , " + _docB.TAXAR);
            if (_docA.VALUTA != _docB.VALUTA) _errors.Add("VALUTA differs First, Second " + _docA.VALUTA + " , " + _docB.VALUTA);

        }


        private void comparePeriodValueList(List<SiePeriodValue> listA, List<SiePeriodValue> listB, string listName, string nameA, string nameB)
        {
            foreach (var pA in listA)
            {
                bool foundIt = false;
                foreach (var pB in listB)
                {
                    if (periodValueComparer(pA, pB))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    _errors.Add(listName + " differs Account, YearNo, Period not found or different in " + nameB + ": " + pA.Account.Number + ", " + pA.YearNr + ", " + pA.Period);
                }
            }
        }

        private bool periodValueComparer(SiePeriodValue a, SiePeriodValue b)
        {
            if (a.Account.Number != b.Account.Number) return false;
            if (a.Amount != b.Amount) return false;
            if (a.Quantity != b.Quantity) return false;
            if (a.Period != b.Period) return false;
            if (a.Token != b.Token) return false;
            if (a.YearNr != b.YearNr) return false;
            if (!compareObjects(a.Objects, b.Objects)) return false;


            return true;
        }
        private void compareProgram(List<string> a, List<string> b)
        {
            bool equal = true;
            if (a != null && b == null) equal = false;
            if (b != null && a == null) equal = false;
            if (a != null && b != null)
            {
                if (a.Count != b.Count)
                {
                    equal = false;
                }
                else
                {
                    for (int i = 0; i < a.Count; i++)
                    {
                        if (a[i] != b[i]) equal = false;
                    }
                }
            }

            if (!equal) _errors.Add($"PROGRAM differs First {string.Join(" ", a)} Second {string.Join(" ", b)}");
        }

        private bool compareObjects(List<SieObject> a, List<SieObject> b)
        {
            if(a==null) a = new List<SieObject>();
            if(b==null) b = new List<SieObject>();

            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].Dimension.Number != b[i].Dimension.Number) return false;
                if (a[i].Name != b[i].Name) return false;
                if (a[i].Number != b[i].Number) return false;
            }
            return true;
        }
        private void compareFNAMN()
        {
            string a, b;

            if (_docA.FNAMN != null && _docB.FNAMN != null)
            {
                if (!strEqual(_docA.FNAMN.Code, _docB.FNAMN.Code)) _errors.Add("FNAMN.Code differs First, Second " + _docA.FNAMN.Code + " , " + _docB.FNAMN.Code);
                if (!strEqual(_docA.FNAMN.Contact, _docB.FNAMN.Contact)) _errors.Add("ADRESS.Contact differs First, Second " + _docA.FNAMN.Contact + " , " + _docB.FNAMN.Contact);
                if (!strEqual(_docA.FNAMN.Name, _docB.FNAMN.Name)) _errors.Add("FNAMN.Name differs First, Second " + _docA.FNAMN.Name + " , " + _docB.FNAMN.Name);
                a = _docA.FNAMN.OrgIdentifier ?? "";
                b = _docB.FNAMN.OrgIdentifier ?? "";
                if (a != b) _errors.Add("ORGNR.OrgIdentifier differs First, Second " + _docA.FNAMN.OrgIdentifier + " , " + _docB.FNAMN.OrgIdentifier);
                if (!strEqual(_docA.FNAMN.OrgType, _docB.FNAMN.OrgType)) _errors.Add("FTYP differs First, Second " + _docA.FNAMN.OrgType + " , " + _docB.FNAMN.OrgType);
                if (!strEqual(_docA.FNAMN.Phone, _docB.FNAMN.Phone)) _errors.Add("ADRESS.Phone differs First, Second " + _docA.FNAMN.Phone + " , " + _docB.FNAMN.Phone);
                if (_docA.FNAMN.SNI != _docB.FNAMN.SNI) _errors.Add("FNAMN.SNI differs First, Second " + _docA.FNAMN.SNI + " , " + _docB.FNAMN.SNI);
                if (!strEqual(_docA.FNAMN.Street, _docB.FNAMN.Street)) _errors.Add("ADRESS.Street differs First, Second " + _docA.FNAMN.Street + " , " + _docB.FNAMN.Street);
                if (!strEqual(_docA.FNAMN.ZipCity, _docB.FNAMN.ZipCity)) _errors.Add("ADRESS.ZipCity differs First, Second " + _docA.FNAMN.ZipCity + " , " + _docB.FNAMN.ZipCity);
            }
            else
            {
                _errors.Add("FNAMN differs.");
            }
        }

        private void compareKONTO(SieDocument docA, SieDocument docB, string nameA, string nameB)
        {
            foreach (var kA in docA.KONTO.Values)
            {
                if (docB.KONTO.ContainsKey(kA.Number))
                {
                    var kB = docB.KONTO[kA.Number];
                    if (!strEqual(kA.Name, kB.Name)) _errors.Add("KONTO.Name differ  " + kA.Number);
                    if (!strEqual(kA.Type, kB.Type)) _errors.Add("KONTO.Type differ " + kA.Number);
                    if (!strEqual(kA.Unit, kB.Unit)) _errors.Add("KONTO.Unit differ " + kA.Number);
                    if (kA.SRU.Count == kB.SRU.Count)
                    {
                        for (int i = 0; i < kA.SRU.Count; i++)
                        {
                            if (kA.SRU.ElementAt(i) != kB.SRU.ElementAt(i))
                            {
                                _errors.Add("KONTO.SRU differ " + kA.Number);
                                break;
                            }
                        }
                    }
                    else
                    {
                        _errors.Add("KONTO.SRU differ " + kA.Number);
                    }

                }
                else
                {
                    _errors.Add(nameB + " is missing KONTO: " + kA.Number);
                }

            }
        }
        private void compareDIM(SieDocument docA, SieDocument docB, string nameA, string nameB)
        {
            foreach (var dimKey in docA.DIM.Keys)
            {
                if (docB.DIM.ContainsKey(dimKey))
                {
                    if (!docA.DIM[dimKey].Name.Equals(docB.DIM[dimKey].Name)) _errors.Add("DIM " + dimKey + " Name differ " + nameA + "," + nameB + ":" + docA.DIM[dimKey].Name + " , " + docB.DIM[dimKey].Name);
                    if (!docA.DIM[dimKey].Number.Equals(docB.DIM[dimKey].Number)) _errors.Add("DIM " + dimKey + " Number differ " + nameA + "," + nameB + ":" + docA.DIM[dimKey].Number + " , " + docB.DIM[dimKey].Number);
                    if (docA.DIM[dimKey].SuperDim != null && docB.DIM[dimKey].SuperDim == null) _errors.Add("DIM " + dimKey + " SuberDim differ " + nameA + " has DIM ," + nameB + " is NULL, ");
                    if (docA.DIM[dimKey].SuperDim != null && docB.DIM[dimKey].SuperDim != null)
                    {
                        if (!docA.DIM[dimKey].SuperDim.Name.Equals(docB.DIM[dimKey].SuperDim.Name)) _errors.Add("DIM " + dimKey + " SuperDim.Name differ " + nameA + "," + nameB + ":" + docA.DIM[dimKey].SuperDim.Name + " , " + docB.DIM[dimKey].SuperDim.Name);
                        if (!docA.DIM[dimKey].SuperDim.Number.Equals(docB.DIM[dimKey].SuperDim.Number)) _errors.Add("DIM " + dimKey + " SuperDim.Number differ " + nameA + "," + nameB + ":" + docA.DIM[dimKey].SuperDim.Number + " , " + docB.DIM[dimKey].SuperDim.Number);
                    }
                }
                else
                {
                    _errors.Add(nameB + " DIM is missing " + dimKey);
                }
            }
        }
        private void compareRAR(SieDocument docA, SieDocument docB, string nameA, string nameB)
        {
            foreach (var rarA in docA.RAR.Values)
            {
                if (docB.RAR.ContainsKey(rarA.ID))
                {
                    var rarB = docB.RAR[rarA.ID];
                    if (rarA.Start != rarB.Start) _errors.Add(nameB + "RAR differs " + rarA.ID);
                    if (rarA.End != rarB.End) _errors.Add(nameB + "RAR differs " + rarA.ID);
                }
                else
                {
                    _errors.Add(nameB + "RAR is missing " + rarA.ID);
                }
            }
        }
        private void compareVER(SieDocument docA, SieDocument docB, string nameA, string nameB)
        {
            foreach (var vA in docA.VER)
            {
                bool foundIt = false;
                foreach (var vB in docB.VER)
                {
                    if (voucherComparer(vA, vB))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    _errors.Add("Vouchers differs Series, Number not found or different in " + nameB + ": " + vA.Series + ", " + vA.Number);
                }
            }
        }

        private bool voucherComparer(SieVoucher vA, SieVoucher vB)
        {
            if (vA.Number != vB.Number) return false;
            if (vA.Series != vB.Series) return false;
            if (vA.Text != vB.Text) return false;
            if(vA.Token != vB.Token) return false;
            if(vA.VoucherDate != vB.VoucherDate) return false;
            if(vA.Rows.Count != vB.Rows.Count)
            {
                return false;
            }
            else
            {
                foreach(var rA in vA.Rows)
                {
                    bool foundIt = true;
                    foreach(var rB in vB.Rows)
                    {
                        if(rA.Account.Number != rB.Account.Number) foundIt = false;
                        if (rA.Amount != rB.Amount) foundIt = false;
                        if (rA.CreatedBy != rB.CreatedBy) foundIt = false;
                        if (rA.RowDate != rB.RowDate) foundIt = false;
                        if (rA.Quantity != rB.Quantity) foundIt = false;
                        if (!compareObjects(rA.Objects, rB.Objects)) foundIt = false;

                        if (foundIt) break;
                    }
                    if(foundIt)
                    {
                        return true;
                    }
                }
            }

            return true;
        }
        private bool strEqual(string a, string b)
        {
            return (a ?? "") == (b ?? "");
        }
    }
}
