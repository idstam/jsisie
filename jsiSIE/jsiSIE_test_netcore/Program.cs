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

            foreach (var f in Directory.GetFiles(testSourceFolder))
            {
                //if (!f.Contains("30")) continue;
                if (f.EndsWith(".err")) continue;

                var sie = new SieDocument();
                sie.ThrowErrors = false;
                sie.IgnoreMissingOMFATTNING = true;

                if (f.Contains("sie%204.SE"))
                {
                    sie.IgnoreRTRANS = true;
                    sie.IgnoreBTRANS = true;
                }
                
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
                    sieB.ThrowErrors = false;
                    sieB.IgnoreMissingOMFATTNING = true;

                    if (f.Contains("sie%204.SE"))
                    {
                        sieB.IgnoreRTRANS = true;
                        sieB.IgnoreBTRANS = true;
                    }

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
                    sieB1.ThrowErrors = false;
                    sieB1.IgnoreMissingOMFATTNING = true;
                    if (f.Contains("sie%204.SE"))
                    {
                        sieB1.IgnoreRTRANS = true;
                        sieB1.IgnoreBTRANS = true;
                    }
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

        private static string findTestFilesFolder()
        {
            var p = Assembly.GetExecutingAssembly().Location;
            while (true)
            {
                var r = Path.Join(p, "README.md");
                if (File.Exists(r))
                {
                    return Path.Join(p, "sie_test_files");
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
