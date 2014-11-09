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
using System.Threading;
using NiL.JS.Core.TypeProxing;
using System.Collections.Generic;

namespace NiL.JSTest
{
    class Program
    {
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
            Script s = null;
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
                if (i != 0 && !skipedShowed)
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

        private static void webkitTests(string folderPath = "tests\\webkit\\")
        {
            bool showAll = false;
            bool refresh = true;
            int lastUpdate = Environment.TickCount;

            Action<string> _ = Console.WriteLine;
            var sw = new Stopwatch();
            string code;
            string preCode = "";
            string postCode = "";
            Script s = null;
            _("webkit testing begun...");
            _("Load standalone-pre.js...");
            using (var staFile = new FileStream(folderPath + "resources\\standalone-pre.js", FileMode.Open, FileAccess.Read))
                preCode = new StreamReader(staFile).ReadToEnd();
            _("Load standalone-post.js...");
            using (var staFile = new FileStream(folderPath + "resources\\standalone-post.js", FileMode.Open, FileAccess.Read))
                postCode = new StreamReader(staFile).ReadToEnd();
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Found " + (fls.Length - 2) + " js-files");
            bool skipedShowed = false;
            sw.Start();
            for (int i = 0; i < fls.Length; i++)
            {
                if (i != 0 && !skipedShowed)
                    _("Skiped: " + i);
                skipedShowed = true;
                try
                {
                    if (fls[i].EndsWith("standalone-pre.js")
                        || fls[i].EndsWith("standalone-post.js"))
                        continue;
                    if (showAll)
                        Console.Write("Processing file \"" + fls[i] + "\" ");
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    code = sr.ReadToEnd();
                    sr.Dispose();
                    f.Dispose();
                    Context econtext = null;
                    if (refresh || s == null)
                    {
                        Context.RefreshGlobalContext();
                        s = new Script(preCode);// инициализация
                        s.Invoke();
                        econtext = s.Context;
                        s.Context.DefineVariable("print").Assign(new ExternalFunction((t, e) =>
                        {
                            var text = e[0].isDefinded ? e[0].ToString() : "";
                            //if (text == "FAIL")
                            //    System.Diagnostics.Debugger.Break();
                            Console.WriteLine(text);
                            return JSObject.Undefined;
                        }));
                    }
                    else
                        econtext = new Context(s.Context);
                    try
                    {
                        econtext.Eval(code);
                    }
                    finally
                    {
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debugger.Break();
                    Console.WriteLine(e);
                }
            }
            sw.Stop();
            _("time: " + sw.Elapsed);
            _("webkit testing complite");
        }

        private static void benchmark()
        {
            Script s = null;
            var sw = new Stopwatch();
            int @case = 2;
            switch (@case)
            {
                case 0:
                    {
                        s = new Script(
            @"
function fib(x)
{
    if (x < 2)
        return 1;
    return fib(x - 1) + fib(x - 2);
}// 420 // 485 // 525 // 600 // 650
for (var i = 0; i < 700; i++) fib(20);
");
                        break;
                    }
                case 1:
                    {
                        s = new Script(
            @"
for (var i = 0; i < 24000000; i++) Math.abs(i);
");
                        break;
                    }
                case 2:
                    {
                        s = new Script(
            @"
function abs(x)
{
    return x < 0 ? -x : x;
}
for (var i = 0; i < 10000000; i++) abs(i * (1 - 2 * (i & 1)));
");
                        break;
                    }
            }
            s.Invoke();
            GC.Collect(0);
            GC.Collect(1);
            GC.Collect(2);
            GC.GetTotalMemory(true);
            sw.Start();
            s.Invoke();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void testEx()
        {
            var sw = new Stopwatch();
            var s = new Script(
@"
test.Add(true);
");
            s.Context.DefineVariable("System").Assign(new NamespaceProvider("System"));
            s.Context.DefineVariable("test").Assign(TypeProxy.Proxy(new List<Arguments>()));
            sw.Start();
            s.Invoke();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private static void strongTests()
        {
            var sw = new Stopwatch();
            var s = new Script(
@"
function strongFunction(i, d, s, o)
{
    'use strong';
    i = i | 0;
    d = +d;
    s += '';
    o = Object(o);
    console.log(typeof i);
    console.log(typeof d);
    console.log(typeof s);
    console.log(typeof o);
}
strongFunction(1, 2, 3, 4);
");
            sw.Start();
            s.Invoke();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            // В текущем процессе часовой пояс будет -8:00:00. 
            // Создатели sputnik'a не удосужились в своих тестах учитывать временную зону 
            // и от всех требуют пребывания в указаном часовом поясе.
            var currentTimeZone = TimeZone.CurrentTimeZone;
            var offset = currentTimeZone.GetType().GetField("m_ticksOffset", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            offset.SetValue(currentTimeZone, new TimeSpan(-8, 0, 0).Ticks);

            Context.GlobalContext.DebuggerCallback += (sender, e) => System.Diagnostics.Debugger.Break();
            Context.GlobalContext.DefineVariable("alert").Assign(new ExternalFunction((t, a) => { System.Windows.Forms.MessageBox.Show(a[0].ToString()); return JSObject.Undefined; }));

            int mode = 155
                   ;
            switch (mode)
            {
                case -5:
                    {
                        strongTests();
                        break;
                    }
                case -4:
                    {
                        runFile(@"samples/async.js");
                        break;
                    }
                case -3:
                    {
                        Context.GlobalContext.DefineVariable
                            ("forms") // имя переменной, через которую будет доступно пространство имён.
                            .Assign(new NamespaceProvider
                                ("System.Windows.Forms")); // пространство имён, к которому будет осуществляться доступ.
                        runFile("samples/WinFormsSample.js");
                        break;
                    }
                case -2:
                    {
                        var bf = new BinaryFormatter();
                        var ms = new MemoryStream();
                        bf.Serialize(ms, new BinaryTree<int>() { { "one", 1 }, { "two", 2 }, { "three", 3 } });
                        ms.Position = 0;
                        var bt = bf.Deserialize(ms);
                        break;
                    }
                case -1:
                    {
                        runFiles("tests/custom/");
                        sputnikTests(@"tests\sputnik\ch15\15.1\");
                        sputnikTests(@"tests\sputnik\ch15\15.2\");
                        sputnikTests(@"tests\sputnik\ch15\15.3\");
                        sputnikTests(@"tests\sputnik\ch15\15.4\"); // with 43 asserts
                        sputnikTests(@"tests\sputnik\ch15\15.5\"); // with some errors due double.toString()
                        sputnikTests(@"tests\sputnik\ch15\15.6\");
                        sputnikTests(@"tests\sputnik\ch15\15.7\");
                        sputnikTests(@"tests\sputnik\ch15\15.8\"); // with some errors due accuracy comparison
                        sputnikTests(@"tests\sputnik\ch15\15.9\");
                        sputnikTests(@"tests\sputnik\ch15\15.11\");
                        sputnikTests(@"tests\sputnik\ch15\15.12\");
                        break;
                    }
                case 0:
                    {
                        runFiles("tests/custom/");
                        sputnikTests();
                        break;
                    }
                case 1:
                    {
                        runFiles("tests/custom/");
                        webkitTests();
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
                        benchmark();
                        break;
                    }
                case 5:
                    {
                        runFile(@"coffee-script.js");
                        break;
                    }
                case 6:
                    {
                        runFile(@"linq.js");
                        break;
                    }
                case 7:
                    {
                        runFile(@"arraytests.js");
                        break;
                    }
                case 8:
                    {
                        runFile(@"handlebars-v2.0.0.js");
                        break;
                    }
                case 9:
                    {
                        Context.GlobalContext.DefineVariable("print").Assign(new ExternalFunction((t, a) =>
                        {
                            for (var i = 0; i < a.Length; i++)
                                System.Console.WriteLine(a[i]);
                            return JSObject.Undefined;
                        }));
                        Context.GlobalContext.DefineVariable("stderr").Assign(true);
                        runFile(@"jsfunfuzz.js");
                        break;
                    }
                case 10:
                    {
                        runFile(@"md5.js");
                        break;
                    }
                case 11:
                    {
                        runFile(@"tests\custom\Int8Array.js");
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
                case 155:
                    {
                        // String
                        sputnikTests(@"tests\sputnik\ch15\15.5\");
                        break;
                    }
                case 156:
                    {
                        // Boolean
                        sputnikTests(@"tests\sputnik\ch15\15.6\");
                        break;
                    }
                case 157:
                    {
                        // Number
                        sputnikTests(@"tests\sputnik\ch15\15.7\");
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
                case 1510:
                    {
                        // RegExp
                        sputnikTests(@"tests\sputnik\ch15\15.10\");
                        break;
                    }
                case 1511:
                    {
                        // Error
                        sputnikTests(@"tests\sputnik\ch15\15.11\");
                        break;
                    }
                case 1512:
                    {
                        // JSON
                        sputnikTests(@"tests\sputnik\ch15\15.12\");
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
                            runFile(@"sunspider-0.9.1\string-base64.js");
                        break;
                    }
                case 103:
                    {
                        kraken();
                        break;
                    }
                case 104:
                    {
                        cryptojs();
                        break;
                    }
            }

            GC.Collect(0);
            GC.Collect(1);
            GC.Collect(2);
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
            else if (Debugger.IsAttached)
                Console.ReadKey();
        }

        private static void cryptojs()
        {
            var folderPath = "crypto-js\\3.1.2";

            Action<string> _ = Console.WriteLine;
            var context = new Context();
            Action<string> load = path =>
            {
                using (var fs = new FileStream(folderPath + "\\" + path, FileMode.Open, FileAccess.Read))
                using (var sr = new StreamReader(fs))
                    context.Eval(sr.ReadToEnd());
            };
            load("src/core.js");
            load("src/lib-typedarrays.js");
            load("src/x64-core.js");
            load("src/enc-utf16.js");
            load("src/enc-base64.js");
            load("src/md5.js");
            load("src/sha1.js");
            load("src/sha256.js");
            load("src/sha224.js");
            load("src/sha512.js");
            load("src/sha384.js");
            load("src/sha3.js");
            load("src/ripemd160.js");
            load("src/hmac.js");
            load("src/pbkdf2.js");
            load("src/evpkdf.js");
            load("src/cipher-core.js");
            load("src/mode-cfb.js");
            load("src/mode-ctr.js");
            load("src/mode-ofb.js");
            load("src/mode-ecb.js");
            load("src/pad-ansix923.js");
            load("src/pad-iso10126.js");
            load("src/pad-zeropadding.js");
            load("src/pad-iso97971.js");
            load("src/pad-nopadding.js");
            load("src/rc4.js");
            load("src/rabbit.js");
            load("src/rabbit-legacy.js");
            load("src/aes.js");
            load("src/tripledes.js");
        }

        private static void runFiles(string folderPath)
        {
            Action<string> _ = Console.WriteLine;
            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Found " + (fls.Length - 2) + " js-files");
            for (var i = 0; i < fls.Length; i++)
                runFile(fls[i]);
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

                    sw.Restart();
                    new Script(script).Invoke();
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

        private static void kraken()
        {
            var folderPath = "kraken-1.1";

            Action<string> _ = Console.WriteLine;
            var sw = new Stopwatch();
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");
            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Founded " + fls.Length + " js-files");

            long _total = 0;
            var round = 0;
            long min = long.MaxValue;
            Context.RefreshGlobalContext();
            for (; round < 1; round++)
            {
                TimeSpan total = new TimeSpan();

                for (var i = 0; i < fls.Length; )
                {
                    string data, body;
                    using (var f = new FileStream(fls[i++], FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(f))
                        data = sr.ReadToEnd();
                    _("Process " + fls[i]);
                    using (var f = new FileStream(fls[i++], FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(f))
                        body = sr.ReadToEnd();

                    sw.Restart();
                    var script = new Script(data);
                    script.Context.DefineVariable("print").Assign(new ExternalFunction((t, a) =>
                    {
                        for (var j = 0; j < a.Length; j++)
                            System.Console.WriteLine(a[j]);
                        return JSObject.Undefined;
                    }));
                    script.Invoke();
                    script.Context.Eval(body);
                    sw.Stop();
                    total += sw.Elapsed;
                    _(sw.Elapsed.ToString());
                    Context.RefreshGlobalContext();
                    GC.Collect(0);
                    GC.Collect(1);
                    GC.Collect(2);
                    GC.GetTotalMemory(true);
                }
                _("Total: " + total);
                _total += total.Ticks;
                min = System.Math.Min(total.Ticks, min);
            }
            _("Average: " + new TimeSpan(_total / round));
            _("Minimum: " + new TimeSpan(min));
        }
    }
}
