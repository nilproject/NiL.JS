using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace NiL.JSTest
{
    class Program
    {
        private static void runFile(string filename)
        {
            Console.WriteLine("Processing file: " + filename);
            var f = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(f);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var s = new Script(sr.ReadToEnd());
            sr.Dispose();
            f.Dispose();
            sw.Stop();
            Console.WriteLine("Compile time: " + sw.Elapsed);
            Console.WriteLine("-------------------------------------");
            s.Invoke();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
        }

        private static void runTestFile(string filename)
        {
            string staCode = "";
            using (var staFile = new FileStream("sta.js", FileMode.Open, FileAccess.Read))
                staCode = new StreamReader(staFile).ReadToEnd();
            Console.WriteLine("Processing file: " + filename);
            var f = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(f);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var s = new Script(staCode);
            sw.Stop();
            Console.WriteLine("Compile time: " + sw.Elapsed);
            sw.Restart();
            s.Invoke();
            sw.Stop();
            Console.WriteLine("Init time: " + sw.Elapsed);
            Console.WriteLine("-------------------------------------");
            s.Context.Eval(sr.ReadToEnd());
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
            sr.Dispose();
            f.Dispose();
        }

        private static void sputnicTests(string folderPath = "tests\\")
        {
            bool showAll = false;
            bool refresh = true;
            int lastUpdate = Environment.TickCount;

            Action<string> _ = Console.WriteLine;
            var sw = new Stopwatch();
            int passed = 0;
            int failed = 0;
            string code;
            bool negative = false;
            string staCode = "";
            Script s = null;
            _("Sputnik testing begin...");
            _("Load sta.js...");
            using (var staFile = new FileStream("sta.js", FileMode.Open, FileAccess.Read))
                staCode = new StreamReader(staFile).ReadToEnd();
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Founded " + fls.Length + " js-files");
            sw.Start();
            for (int i = 0; i < fls.Length; i++)
            {
                //if (i < 1033)
                //continue;
                bool pass = true;
                try
                {
                    if (showAll)
                        Console.Write("Processing file \"" + fls[i] + "\" ");
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    code = sr.ReadToEnd();
                    sr.Dispose();
                    f.Dispose();
                    negative = code.IndexOf("@negative") != -1;
                    Context econtext = null;
                    if (refresh || s == null)
                    {
                        Context.RefreshGlobalContext();
                        s = new Script(staCode);
//                        s = new Script(
//@"function runTestCase(f){ if (!f()) throw false; }
//function fnGlobalObject(){ return this; }
//function $PRINT(message){  }
//function $ERROR(message){ throw message; }
//function fnExists(/*arguments*/) {
//    for (var i = 0; i < arguments.length; i++) {
//        if (typeof (arguments[i]) !== ""function"") return false;
//    }
//    return true;
//}
//function Test262Error(message) {
//    if (message) this.message = message;
//}
//
//Test262Error.prototype.toString = function () {
//    return ""Test262 Error: "" + this.message;
//};
//
//function testFailed(message) {
//    throw new Test262Error(message);
//}"); // инициализация
                        s.Invoke();
                        econtext = s.Context;
                    }
                    else
                        econtext = new Context(s.Context);
                    try
                    {
                        econtext.Eval(code);
                    }
                    finally
                    {
                        pass ^= negative;
                    }
                }
                catch (JSException e)
                {
                    pass = negative;
                    if (!pass)
                        Console.WriteLine(e.Message);
                }
                catch (Exception)
                {
                    pass = false;
                }
                if (pass)
                {
                    if (showAll)
                        _("Passed");
                    passed++;
                }
                else
                {
                    if (!showAll)
                        _("File: \"" + fls[i] + "\"" + Environment.NewLine);
                    else
                        _("Failed");
                    failed++;
                }
                if (Environment.TickCount - lastUpdate > 100)
                {
                    Console.Title = "passed: " + passed + ". failed: " + failed;
                    lastUpdate = Environment.TickCount;
                }
                //if (failed == 3) break;
            }
            sw.Stop();
            _("passed: " + passed + ". (" + (passed * 100 / fls.Length) + "%)");
            _("failed: " + failed);
            _("time: " + sw.Elapsed);
            _("Sputnik testing complite");
        }

        private sealed class DoubleStringConverter : NiL.JS.Core.Modules.ConvertValueAttribute
        {
            public override object From(object source)
            {
                return ((double)source).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            public override object To(object source)
            {
                return double.Parse(source as string, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private enum TestEnum : ulong
        {
            Default = 0,
            One = 1
        }

        private struct TestStruct
        {
            public int value;

            public object test()
            {
                return value;
            }

            public object test(int i)
            {
                return i;
            }
        }

        [Immutable]
        private class TestClass
        {
            [DoubleStringConverter]
            public double Value
            {
                get { return 1.0; }

                set { }
            }

            public object test()
            {
                return null;
            }

            public object test(int i)
            {
                return i;
            }
        }

        private static void testEx()
        {
            var sw = new Stopwatch();
            var s = new Script(
@"
var x = 1;
console.log({ x : 2, f:function(){ return function(){ return this.x; } } }.f()());
");
            s.Context.AttachModule(typeof(TestClass));
            s.Context.AttachModule(typeof(TestEnum));
            s.Context.AttachModule(typeof(TestStruct));
            s.Context.AttachModule(typeof(System.Drawing.Point));
            sw.Start();
            s.Invoke();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        static void Main(string[] args)
        {
            typeof(System.Windows.Forms.Button).GetType();
            int mode = 0
                ;
            switch (mode)
            {
                case 0:
                    {
                        sputnicTests();
                        break;
                    }
                case 1:
                    {
                        runTestFile(@"ftest.js");
                        break;
                    }
                case 2:
                    {
                        runFile(@"ftest.js");
                        break;
                    }
                case 3:
                    {
                        testEx();
                        break;
                    }
                case 4:
                    {
                        // Object
                        sputnicTests(@"tests\sputnik\ch15\15.2\");
                        break;
                    }
                case 5:
                    {
                        // Array
                        sputnicTests(@"tests\sputnik\ch15\15.4\");
                        break;
                    }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("GC.GetTotalMemory: " + GC.GetTotalMemory(true));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(0));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(1));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(2));
            Console.WriteLine("GC.MaxGeneration: " + GC.MaxGeneration);
            if (System.Windows.Forms.Application.OpenForms.Count != 0)
            {
                while (System.Windows.Forms.Application.OpenForms.Count != 0)
                {
                    System.Threading.Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            else Console.ReadKey();
        }
    }
}
