using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.Core.Interop;

namespace NetCoreTestApp
{
    public class Program
    {
        public class MyObject
        {
            public NiL.JS.BaseLibrary.Array getCodes()
            {
                var gc = Context.CurrentContext.GlobalContext;
                return new NiL.JS.BaseLibrary.Array(MakeCodes());
            }

            private IList<long> MakeCodes()
            {
                return (new List<long>() { 1, 2, 3, 4, 5 }).Where(x => x > 0).ToList();
            }

            public void takeCodes()
            {
            }

            public void takeCodes(int x)
            {
            }

            public void takeCodes(double[] codes)
            {
                for (int i = 0; i < codes.Length; i++)
                {
                    Console.WriteLine("[{0}]: {1}", i, codes[i]);
                }
            }

            public void takeCodes(long[] codes)
            {
                for (int i = 0; i < codes.Length; i++)
                {
                    Console.WriteLine("[{0}]: {1}", i, codes[i]);
                }
            }
        }

        static void Main(string[] args)
        {
            new Module(File.ReadAllText("tests/modules/ftest.js")).Run();
        }

        private static void sunspider()
        {
            var folderPath = "tests/sunspider-0.9.1";

            Action<string> _ = Console.WriteLine;
            var sw = new Stopwatch();
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");
            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Found " + fls.Length + " js-files");

            long _total = 0;
            var round = 0;
            long min = long.MaxValue;
            for (; round < 10; round++)
            {
                TimeSpan total = new TimeSpan();

                for (var i = 0; i < fls.Length; i++)
                {
                    _("Process " + fls[i]);
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    var script = sr.ReadToEnd();
                    sr.Dispose();
                    f.Dispose();

                    var m = new Module(script);

                    sw.Restart();
                    m.Run();
                    sw.Stop();

                    total += sw.Elapsed;
                    _(sw.Elapsed.ToString());
                }
                _("Total: " + total);
                _total += total.Ticks;
                min = System.Math.Min(total.Ticks, min);
                GC.GetTotalMemory(true);
            }
            _("Average: " + new TimeSpan(_total / round));
            _("Minimum: " + new TimeSpan(min));
        }

        private static void sputnikTests(string folderPath = "tests\\sputnik\\")
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
            Module s = null;

            _("Sputnik testing begun...");

            _("Load sta.js...");
            using (var staFile = new FileStream("sta.js", FileMode.Open, FileAccess.Read))
                staCode = new StreamReader(staFile).ReadToEnd();

            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");
            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Found " + fls.Length + " js-files");

            bool skipedShowed = false;
            sw.Start();
            for (int i = 0; i < fls.Length; i++)
            {
                if (i != (i ^ i) && !skipedShowed)
                    _("Skiped: " + i);
                skipedShowed = true;
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
                        Context.ResetGlobalContext();
                        s = new Module(staCode);// инициализация
                        s.Run();
                        econtext = s.Context;
                    }
                    else
                        econtext = new Context(s.Context);
                    try
                    {
                        econtext.Eval(code, true);
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
                catch (Exception e)
                {
                    System.Diagnostics.Debugger.Break();
                    Console.WriteLine(e);
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
            _("Sputnik test complite");
        }
    }
}
