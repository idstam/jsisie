using jsiSIE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

            switch (args[0])
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

            foreach (var err in result)
            {
                Console.WriteLine(err);
            }
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to close.");
            Console.ReadLine();
        }

        private static void BootstrapTest()
        {
            string testSourceFolder = findTestFilesFolder();

            var ignoreFormatMissmatch = true;
            var ignoreProgramMissmatch = true;

            foreach (var f in Directory.GetFiles(testSourceFolder))
            {
                //if (!f.Contains("30")) continue;
                if (f.EndsWith(".err")) continue;

                var sie = new SieDocument();
                sie.ThrowErrors = false;
                sie.IgnoreMissingOMFATTNING = true;

                SetFileSpecificSettings(f, sie);

                if (f.Contains("transaktioner_ovnbolag-bad-balance"))
                {
                    sie.AllowUnbalancedVoucher = true;
                }

                sie.ReadDocument(f);

                if (sie.ValidationExceptions.Count()  > 0)
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
                    sieB.ThrowErrors = false;
                    sieB.IgnoreMissingOMFATTNING = true;


                    SetFileSpecificSettings(f, sieB);

                    sieB.ReadDocument(testWriteFile);
                    var compErrors = SieDocumentComparer.Compare(sie, sieB);
                    foreach (var e in compErrors)
                    {
                        if (ignoreFormatMissmatch && e.Contains("FORMAT differs")) continue;
                        if (ignoreProgramMissmatch && e.Contains("PROGRAM differs")) continue;

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
                    sieB1.ThrowErrors = false;
                    sieB1.IgnoreMissingOMFATTNING = true;

                    SetFileSpecificSettings(f, sieB1);

                    sieB1.ReadDocument(testWriteFile1);
                    var compErrors1 = SieDocumentComparer.Compare(sie, sieB1);
                    foreach (var e in compErrors1)
                    {
                        if (ignoreFormatMissmatch && e.Contains("FORMAT differs")) continue;
                        if (ignoreProgramMissmatch && e.Contains("PROGRAM differs")) continue;
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

        private static void SetFileSpecificSettings(string filename, SieDocument doc)
        {
            if (filename.Contains("sie%204.SE"))
            {
                doc.IgnoreRTRANS = true;
                doc.IgnoreBTRANS = true;
            }

            if (filename.Contains("underdim.SE"))
            {
                doc.AllowUnderDimensions = true;
            }
        }

        private static string findTestFilesFolder()
        {
            var p = Assembly.GetExecutingAssembly().Location;
            while (true)
            {
                var r = Path.Combine(p, "README.md");
                if (File.Exists(r))
                {
                    return Path.Combine(p, "sie_test_files");
                }

                var di = new DirectoryInfo(p);
                if(di.Parent == null)
                {
                    throw new DirectoryNotFoundException("Couldn't find test file folder");
                }
                p = di.Parent.FullName;
            }
        }
    }
}
