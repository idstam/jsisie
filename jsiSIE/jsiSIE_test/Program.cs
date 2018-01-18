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
            if (args.Length == 0)
            {
                BootstrapTest();
                return;
            }

            switch(args[0])
            {
                case "COMPARE":
                    Compare(args);
                    break;
            }

        }

        private static void Compare(string[] args)
        {
            Console.WriteLine("Comparing: ");
            Console.WriteLine(args[1]);
            Console.WriteLine(args[2]);
            var fileA = args[1];
            var fileB = args[2];
            var docA = new SieDocument() { ThrowErrors = false, IgnoreMissingOMFATTNING = true };
            var docB = new SieDocument() { ThrowErrors = false, IgnoreMissingOMFATTNING = true };
            docA.ReadDocument(fileA);
            docB.ReadDocument(fileB);
            var result = SieDocumentComparer.Compare(docA, docB);

            foreach(var err in result)
            {
                Console.WriteLine(err);
            }
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to close.");
            Console.ReadLine();
        }

        private static void BootstrapTest()
        {
            string testSourceFolder = @"c:\temp\sie_test_files";
            if (!Directory.Exists(testSourceFolder)) Directory.CreateDirectory(testSourceFolder);

            GetExampleFiles(testSourceFolder);

            foreach (var f in Directory.GetFiles(testSourceFolder))
            {
                //if (!f.Contains("30")) continue;
                if (f.EndsWith(".err")) continue;

                var sie = new SieDocument();
                sie.ThrowErrors = false;
                sie.IgnoreMissingOMFATTNING = true;

                sie.ReadDocument(f);
                if (sie.ValidationExceptions.Count > 0)
                {
                    foreach (var ex in sie.ValidationExceptions)
                    {
                        Console.WriteLine(f);
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                }
                else
                {

                    var testWriteFile = Path.Combine(testSourceFolder, "testWrite.se");
                    if (File.Exists(testWriteFile)) File.Delete(testWriteFile);

                    var writer = new SieDocumentWriter(sie);
                    writer.Write(testWriteFile);

                    var sieB = new SieDocument();
                    sieB.IgnoreMissingOMFATTNING = true;
                    sieB.ReadDocument(testWriteFile);
                    var compErrors = SieDocumentComparer.Compare(sie, sieB);
                    foreach (var e in compErrors)
                    {
                        Console.WriteLine(e);
                    }
                    Console.WriteLine(f);

                    var testWriteFile1 = Path.Combine(testSourceFolder, "testWrite1.se");
                    if (File.Exists(testWriteFile1)) File.Delete(testWriteFile1);

                    var writer1 = new SieDocumentWriter(sie);
                    using (var s = File.OpenWrite(testWriteFile1))
                    {
                        writer1.Write(s);
                    }

                    var sieB1 = new SieDocument();
                    sieB1.IgnoreMissingOMFATTNING = true;
                    sieB1.ReadDocument(testWriteFile1);
                    var compErrors1 = SieDocumentComparer.Compare(sie, sieB1);
                    foreach (var e in compErrors1)
                    {
                        Console.WriteLine(e);
                    }
                    Console.WriteLine(f);
                }
                //break;
            }
            Console.WriteLine();
            Console.WriteLine("Press ENTER to quit.");
            Console.ReadLine();
        }

        private static void GetExampleFiles(string testSourceFolder)
        {
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
                        wc.DownloadFile(url, fileName);
                        Console.WriteLine(" OK");
                    }
                    catch (Exception ex)
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
