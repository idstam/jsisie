using jsiSIE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace jsiSIE_test
{
    class Program
    {
        static void Main(string[] args)
        {
            string testSourceFolder = @"c:\temp\sie_test_files";
            if (!Directory.Exists(testSourceFolder)) Directory.CreateDirectory(testSourceFolder);

            //Download all the existing SIE test files if you don't have them.
            if (Directory.GetFiles(testSourceFolder).Count() == 0)
            {
                var wc = new WebClient();
                int i = 0;
                foreach (var url in TestFilesOnline())
                {
                    var uri = new Uri(url);
                    var fileName = Path.GetFileName(uri.LocalPath);
                    fileName = Path.Combine(testSourceFolder, i.ToString() + "_" + fileName);
                    //Add a counter to the filename to handle that files have the same name with different case.
                    try
                    {
                        Console.Write("Getting: " + url);
                        wc.DownloadFile(url,fileName);
                        Console.WriteLine(" OK");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(" FAIL");
                        File.WriteAllText(fileName + ".err", ex.ToString());
                    }
                    i++;
                }
            }
            else
            {
                Console.WriteLine("There are already test files. No need to download.");
            }

            foreach(var f in Directory.GetFiles(testSourceFolder))
            {
                if (f.EndsWith(".err")) continue;

                //if (SieDocument.GetSieVersion(f) != 4) continue;
                //if (!f.Contains("37_Norstedts Bokslut SIE 1")) continue;

                var sie = new SieDocument(f);
                sie.ThrowErrors = false;
                //sie.IgnoreMissingOMFATTNING = true;

                sie.ReadDocument();
                if(sie.ValidationExceptions.Count > 0)
                {
                    foreach(var ex in sie.ValidationExceptions)
                    {
                        Console.WriteLine(f);
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Press ENTER to quit.");
            Console.ReadLine();

        }
        private static List<string> TestFilesOnline()
        {
            var ret = new List<string>();
            //BL Administration
            ret.Add("http://www.sie.se/wp-content/uploads/files/BL0001_typ1.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/BL0001_typ2.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/BL0001_typ3.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/BL0001_typ4.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/BL0001_typ4I.SI");

            //Briljant
            ret.Add("http://www.sie.se/wp-content/uploads/files/Test1.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Test2.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Test3.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Test4.SE");

            //e-conomic
            ret.Add("http://www.sie.se/wp-content/uploads/files/sie-gruppen-e-conomic.se");

            //Edison Bokföring
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ1.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ2.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ4.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ4si.si");

            //Edison Ekonomi Byrå
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ1.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ2.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ3.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ4.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/typ4si.si");

            //Fortnox Bokföring
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie1.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie2.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie3.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie4.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie4.si");

            //Hogia Affärssystem
            ret.Add("http://www.sie.se/wp-content/uploads/files/HAS1_1412.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/HAS2_1412.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/HAS3_1412.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/HAS4E_1412.Se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/HAS4i_1412.si");

            //Hogia Small Office Bokföring
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie%201+2.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Sie%203%20+%204.se");

            //Kontek Lön
            ret.Add("http://www.sie.se/wp-content/uploads/files/LON%20L%C3%B6nek%C3%B6rning.SI");

            //Mamut One Enterprise
            ret.Add("http://www.sie.se/wp-content/uploads/files/MAMUT_SIE1_EXPORT.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/MAMUT_SIE2_EXPORT.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/MAMUT_SIE3_EXPORT.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/MAMUT_SIE4_EXPORT.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/MAMUT_SIE4_EXPORT.SE");

            //Norstedts Bokslut
            ret.Add("http://www.sie.se/wp-content/uploads/files/Norstedts%20Bokslut%20SIE%201.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Bokslut%20Norstedts%20SIE%204E.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/Norstedts%20Bokslut%20SIE%204I.si");

            //Norstedts Revision
            ret.Add("http://www.sie.se/wp-content/uploads/files/Norstedts%20Revision%20SIE%201.SE");

            //Pyramid Business Studio
            ret.Add("http://www.sie.se/wp-content/uploads/files/sie1.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/sie2.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/sie3.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/sie4.se");

            //REAL Fastighetssystemet
            ret.Add("http://www.sie.se/wp-content/uploads/files/Exempelbolaget_SIE_110322_B_33.si");

            //StepOne
            ret.Add("http://www.sie.se/wp-content/uploads/files/BokSald.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/PerSald.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/ObjSald.SE");
            ret.Add("http://www.sie.se/wp-content/uploads/files/TRANSAK.SE");

            //Visma Anläggningsregister
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE4%20Visma%20Anl%C3%A4ggningsregister.si");

            //Visma Avendo Bokföring
            ret.Add("http://www.sie.se/wp-content/uploads/files/arsaldo_ovnbolag.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/periodsaldo_ovnbolag.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/objektsaldo_ovnbolag.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/transaktioner_ovnbolag.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/urval_ovnbolag.si");

            //Visma Bokslut
            ret.Add("http://www.sie.se/wp-content/uploads/files/BokslutSIE1.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/BokOrder.si");

            //Visma Compact 1500
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE1.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE2.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE3.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE4.se");
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE4.se");

            //Visma eEkonomi
            ret.Add("http://www.sie.se/wp-content/uploads/files/live2011.se");

            //Visma Eget Aktiebolag
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE-fil%20fr%C3%A5n%20Visma%20Eget%20Aktiebolag%202010.se");

            //Visma Enskild Firma
            ret.Add("http://www.sie.se/wp-content/uploads/files/SIE-fil%20fr%C3%A5n%20Visma%20Enskild%20Firma%202010.se");

            //Visma Lön 100
            ret.Add("http://www.sie.se/wp-content/uploads/files/L%C3%B6n.si");

            return ret;
        }
    }
}
