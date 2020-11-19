using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE
{
    /// <summary>
    /// This is a C# implementation of the CRC algorithm published in the Sie specification
    /// </summary>
    internal class SieCRC32
    {
        public bool Started = false;
        private UInt32 CRC32_POLYNOMIAL = 0xEDB88320;

        private UInt32[] CRCTable = new UInt32[256];
        private UInt32 crc; // Global variabel för att ackumulera CRC 

        // Denna rutin skall anropas för att initiera lookup-tabellen 
        // innan beräkningen påbörjas 

        public SieCRC32()
        {
            CRC_skapa_tabell();
        }


        private void CRC_skapa_tabell()
        {
            UInt32 i;
            UInt32 j;
            UInt32 crc;
            for (i = 0; i <= 255; i++)
            {
                crc = i;
                for (j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ CRC32_POLYNOMIAL;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                } CRCTable[i] = crc;
            }
        }

        // Denna rutin påbörjar CRC-beräkning void 
        public void Start()
        {
            // Påbörjar CRC-beräkning genom att sätta den globala
            // CRC-ackumulatorn till prekonditioneringsvärdet. 
            crc = 0xFFFFFFFF;
            Started = true;
        }

        public void AddData(SieDataItem item)
        {
            var buffer = new List<byte>();
            var encoder = Encoding.GetEncoding(437);
            buffer.AddRange(encoder.GetBytes(item.ItemType));

            foreach(var d in item.Data)
            {
                var foo = d.Replace("{", "").Replace("}", "");
                buffer.AddRange(encoder.GetBytes(foo));    
            }
            CRC_ackumulera(buffer);
        }
        public UInt32 Checksum()
        {
            return (crc ^ 0xFFFFFFFF);
        }
        // Denna rutin anropas för varje textdel som ska ingå 
        // i kontrollsumman void 
        private void CRC_ackumulera(List<byte> buffer)
        {

            UInt32 temp1;
            UInt32 temp2;
            foreach (byte p in buffer)
            {
                temp1 = (crc >> 8) & 0x00FFFFFF;
                temp2 = CRCTable[((int)crc ^ p) & 0xff];
                crc = temp1 ^ temp2;
            }
        }
    }
}
