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
        private static void sputnikTests(string folderPath = "tests\\")
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
            _("Sputnik testing begгn...");
            _("Load sta.js...");
            using (var staFile = new FileStream("sta.js", FileMode.Open, FileAccess.Read))
                staCode = new StreamReader(staFile).ReadToEnd();
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Found " + fls.Length + " js-files");
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
                        s = new Script(staCode);// инициализация
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
                    System.Diagnostics.Debugger.Break();
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
                //if (failed == 1)
                //    break;
            }
            sw.Stop();
            _("passed: " + passed + ". (" + (passed * 100 / fls.Length) + "%)");
            _("failed: " + failed);
            _("time: " + sw.Elapsed);
            _("Sputnik testing complite");
        }

        private static void testEx()
        {
            var sw = new Stopwatch();
            var s = new Script(
@"
function fib(x)
{
    if (x < 2)
        return 1;
    return fib(x - 1) + fib(x - 2);
}
console.log(fib(10));
var a = 1;
var b = 2;
var c = 3;
a += function(){ return Math.floor(a * 2 + b * 3 + c * 5) }();
a += function(){ return Math.floor(a * 2 + b * 3 + c * 5) }();
console.log(a);
");
            s.Context.AttachModule(typeof(System.Drawing.Point));
            sw.Start();
            s.Invoke();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        static void Main(string[] args)
        {
            // В текущем процессе часовой пояс будет -8:00:00. 
            // Создатели sputnik'a не удосужились в своих тестах учитывать временную зону 
            // и от всех требуют пребывания в указаном часовом поясе.
            var currentTimeZone = TimeZone.CurrentTimeZone;
            var offset = currentTimeZone.GetType().GetField("m_ticksOffset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            offset.SetValue(currentTimeZone, new TimeSpan(-8, 0, 0).Ticks);

            typeof(System.Windows.Forms.Button).GetType(); // Заставляет подгрузить сборку System.Windows.Forms. Это исключительно для баловства

            int mode = 101
                ;
            switch (mode)
            {
                case 0:
                    {
                        sputnikTests();
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
                case 151:
                    {
                        // Global
                        sputnikTests(@"tests\sputnik\ch15\15.1\");
                        break;
                    }
                case 152:
                    {
                        // Object
                        sputnikTests(@"tests\sputnik\ch15\15.2\");
                        break;
                    }
                case 153:
                    {
                        // Function
                        sputnikTests(@"tests\sputnik\ch15\15.3\");
                        break;
                    }
                case 154:
                    {
                        // Array
                        sputnikTests(@"tests\sputnik\ch15\15.4\");
                        break;
                    }
                case 158:
                    {
                        // Math
                        sputnikTests(@"tests\sputnik\ch15\15.8\");
                        break;
                    }
                case 159:
                    {
                        // Date
                        sputnikTests(@"tests\sputnik\ch15\15.9\");
                        break;
                    }
                case 100:
                    {
                        Context.GlobalContext.DefineVariable("load").Assign(new ExternalFunction((_th, e) =>
                        {
                            using (var f = new FileStream("v8\\" + e["0"], FileMode.Open, FileAccess.Read))
                            {
                                using (var sr = new StreamReader(f))
                                    Context.CurrentContext.Eval(sr.ReadToEnd());
                            }
                            return null;
                        }));

                        runFile(@"v8\run.js");
                        break;
                    }
                case 101:
                    {
                        sunspider();
                        break;
                    }
                case 102:
                    {
                        for (var i = 0; i < 10; i++)
                            runFile(@"sunspider-0.9.1\string-tagcloud.js");
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
            else
                Console.ReadKey();
        }

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
            sw.Restart();
            s.Invoke();
            sw.Stop();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
            Console.WriteLine("Time: " + sw.Elapsed);
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

        private static void sunspider()
        {
            var folderPath = "sunspider-0.9.1";

            Action<string> _ = Console.WriteLine;
            var sw = new Stopwatch();
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");
            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Founded " + fls.Length + " js-files");

            long _total = 0;
            var round = 0;
            for (; round < 5; round++)
            {
                TimeSpan total = new TimeSpan();

                for (var i = 0; i < fls.Length; i++)
                {
                    _("Process " + fls[i]);
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    var script = new Script(sr.ReadToEnd());
                    sr.Dispose();
                    f.Dispose();

                    sw.Restart();
                    script.Invoke();
                    sw.Stop();
                    total += sw.Elapsed;
                    _(sw.Elapsed.ToString());
                }
                _("Total: " + total);
                _total += total.Ticks;
                GC.GetTotalMemory(true);
            }
            _("Average: " + new TimeSpan(_total / round));
        }
    }
}
