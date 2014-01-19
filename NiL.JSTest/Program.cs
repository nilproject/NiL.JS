using NiL.JS;
using NiL.JS.Core;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace NiL.JSTest
{
    class Program
    {
        private static void benchmark()
        {
            const int iterations = 500000000;
            Console.WriteLine("iterations count: " + iterations);

            long init = DateTime.Now.Ticks;
            Script s = new Script(@"
var a = 1; for(var i = 0; i < " + iterations + @";i++){ a = a * 3 * i; }
");
            init = DateTime.Now.Ticks - init;
            long start = DateTime.Now.Ticks;
            s.Invoke();
            long l = (DateTime.Now.Ticks - start);
            Console.WriteLine("script: " + (l / 10000).ToString());
            Console.WriteLine("initialization: " + (init / 10000).ToString());
            var a = 1;
            long nativeStart = DateTime.Now.Ticks;
            for (var i = 0; i < iterations; i++)
                a = a * 3 * i;
            long nativeL = (DateTime.Now.Ticks - nativeStart);
            Console.WriteLine(a);
            Console.WriteLine("native: " + (nativeL / 10000).ToString());
            Console.WriteLine("rate: " + ((double)l / (double)nativeL).ToString());
        }

        private static void featureSupportTest()
        {
            Script s = new Script(@"

");
            s.Invoke(); 
        }

        private static void runFile(string filename)
        {
            Console.WriteLine("Processing file: " + filename);
            Console.WriteLine("-------------------------------------");
            var f = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(f);
            var s = new Script(sr.ReadToEnd());
            s.Context.GetField("$ERROR").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Context.GetField("ERROR").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Context.GetField("$PRINT").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("PRINT: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Invoke();
            sr.Dispose();
            f.Dispose();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
        }

        private static void sputnicTests(string folderPath = "tests\\")
        {
            Action<string> _ = Console.WriteLine;

            int passed = 0;
            int failed = 0;
            string code;
            bool negative = false;
            _("Sputnik testing begin...");
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Founded " + fls.Length + " js-files");

            for (int i = 0; i < fls.Length; i++)
            {
                bool pass = true;
                try
                {
                    Console.Write("Processing file \"" + fls[i] + "\" ");
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    code = "function runTestCase(a){a()}\n" + sr.ReadToEnd();
                    negative = code.IndexOf("* @negative") != -1;
                    if (negative)
                        pass = false;
                    var s = new Script(code);
                    s.Context.GetField("$ERROR").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                        pass = false;
                        return null;
                    }));
                    s.Context.GetField("ERROR").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                        pass = false;
                        return null;
                    }));
                    s.Context.GetField("$PRINT").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("PRINT: " + x.GetField("0", true, false).Value);
                        return null;
                    }));
                    s.Invoke();
                    sr.Dispose();
                    f.Dispose();
                }
                catch (NotImplementedException e)
                {
                    pass = false;
                }
                catch (Exception e)
                {
                    if (!negative)
                        pass = false;
                    else
                        pass = true;
                }
                if (pass)
                {
                    _("Passed");
                    passed++;
                }
                else
                {
                    _("Failed");
                    failed++;
                }
            }

            _("passed: " + passed);
            _("failed: " + failed);

            _("Sputnik testing complite");
            Console.ReadLine();
        }

        private class TestClass
        {
            public float dfield { get { return 1.234f; } }
            public static float sfield = 2.468f;

            public static void smethod()
            {

            }

            public void dmethod()
            {

            }
        }

        private static void testEx()
        {
            Context.GlobalContext.GetField("f").Assign(new CallableField((c, a) => { return null; }));
            Context.GlobalContext.AttachModule(typeof(TestClass));
            var s = new Script("f()");
            s.Invoke();
        }

        static void Main(string[] args)
        {
            NiL.JS.Core.Context.GlobalContext.GetField("platform").Assign("NiL.JS");
            //runFile(@"tests.js");
            //runFile(@"C:\Users\Дмитрий\Documents\Projects\NiL.JS\NiL.JSTest\tests\Conformance\09_Type_Conversion\9.3_ToNumber\9.3.1_ToNumber_from_String\S9.3.1_A2.js");
            //benchmark();
            //featureSupportTest();
            runFile(@"ftest.js");
            //sputnicTests();
            //testEx();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(0));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(1));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(2));
            Console.WriteLine("GC.MaxGeneration: " + GC.MaxGeneration);
            Console.WriteLine("GC.GetTotalMemory: " + GC.GetTotalMemory(false));
        }
    }
}
