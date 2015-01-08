using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    [Serializable]
    public class SieCompany
    {
        /// <summary>
        /// The organisation type names as set by Bolagsverket
        /// </summary>
        private Dictionary<string, string> organisationTypeNames = new Dictionary<string, string>();
        public SieCompany()
        {
            loadOrgTypeNames();
        }

        /// <summary>
        /// #BKOD
        /// </summary>
        public int SNI { get; set; }

        /// <summary>
        /// #FNAMN
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// #FNR
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// #FTYP
        /// </summary>
        public string OrgType { get; set; }

        
        /// <summary>
        /// #ORGNR
        /// </summary>
        public string OrgIdentifier { get; set; }
        /// <summary>
        /// #ADRESS
        /// </summary>
        public string Contact { get; set; }
        public string Street { get; set; }
        public string ZipCity { get; set; }
        public string Phone { get; set; }


        private void loadOrgTypeNames()
        {
            organisationTypeNames.Add("AB", "Aktiebolag.");
            organisationTypeNames.Add("E", "Enskild näringsidkare.");
            organisationTypeNames.Add("HB", "Handelsbolag.");
            organisationTypeNames.Add("KB", "Kommanditbolag.");
            organisationTypeNames.Add("EK", "Ekonomisk förening.");
            organisationTypeNames.Add("KHF", "Kooperativ hyresrättsförening.");
            organisationTypeNames.Add("BRF", "Bostadsrättsförening.");
            organisationTypeNames.Add("BF", "Bostadsförening.");
            organisationTypeNames.Add("SF", "Sambruksförening.");
            organisationTypeNames.Add("I", "Ideell förening som bedriver näring.");
            organisationTypeNames.Add("S", "Stiftelse som bedriver näring.");
            organisationTypeNames.Add("FL", "Filial till utländskt bolag.");
            organisationTypeNames.Add("BAB", "Bankaktiebolag.");
            organisationTypeNames.Add("MB", "Medlemsbank.");
            organisationTypeNames.Add("SB", "Sparbank.");
            organisationTypeNames.Add("BFL", "Utländsk banks filial.");
            organisationTypeNames.Add("FAB", "Försäkringsaktiebolag.");
            organisationTypeNames.Add("OFB", "Ömsesidigt försäkringsbolag.");
            organisationTypeNames.Add("SE", "Europabolag.");
            organisationTypeNames.Add("SCE", "Europakooperativ.");
            organisationTypeNames.Add("TSF", "Trossamfund.");
            organisationTypeNames.Add("X", "Annan företagsform.");
        }

    }
}
