using NiL.JS.Core;
using NiL.JS.Core.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestsDownloader
{
    class Program
    {
        private static readonly string[] TestsSource = new[]
        {
            @"http://test262.ecmascript.org/json/ch06.json",
            @"http://test262.ecmascript.org/json/ch07.json",
            @"http://test262.ecmascript.org/json/ch08.json",
            @"http://test262.ecmascript.org/json/ch09.json",
            @"http://test262.ecmascript.org/json/ch10.json",
            @"http://test262.ecmascript.org/json/ch11.json",
            @"http://test262.ecmascript.org/json/ch12.json",
            @"http://test262.ecmascript.org/json/ch13.json",
            @"http://test262.ecmascript.org/json/ch14.json",
            @"http://test262.ecmascript.org/json/ch15.json",
            @"http://test262.ecmascript.org/json/annexB.json",
        };

        private static void saveTest(string rootDir, JSObject testObj)
        {
            var code = Convert.FromBase64String(testObj["code"].ToString());
            var commentary = testObj["commentary"].ToString();
            var description = testObj["description"].ToString();
            var path = rootDir + testObj["path"];
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                file.Write(code, 0, code.Length);
        }

        private static string[] getTestSet(string url)
        {
            string title = "downloading: " + url + " ...";
            int line = Console.CursorTop;
            Console.Write(title);
            WebRequest wr = HttpWebRequest.Create(url);
            using (var response = wr.GetResponse())
            {
                var data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var tests = JSON.parse(data).GetMember("testSuite");
                var host = response.ResponseUri.Scheme + "://" + response.ResponseUri.Host + "/";
                var res = new string[(int)(tests["length"].Value)];
                int index = 0;
                foreach (var i in tests)
                {
                    res[index++] = host + tests[i].Value.ToString();
                }
                Console.SetCursorPosition(0, line);
                Console.WriteLine(url + " Complete.       ");
                return res;
            }
        }

        private static void download(string url, string rootDir)
        {
            string title = "downloading: " + url + " ...";
            Console.Write(title);
            WebRequest wr = HttpWebRequest.Create(url);
            using (var response = wr.GetResponse())
            {
                var data = new StreamReader(response.GetResponseStream()).ReadToEnd();
                int line = Console.CursorTop;
                title = "saving: " + url;
                Console.SetCursorPosition(0, line);
                Console.Write(title + "         ");
                var tests = JSON.parse(data).GetMember("testsCollection");
                var testsCount = Tools.JSObjectToDouble(tests.GetMember("numTests")) * 0.01;
                tests = tests.GetMember("tests");
                double index = 0;
                foreach (var i in tests)
                {
                    Console.SetCursorPosition(title.Length + 1, line);
                    Console.Write((index++ / testsCount).ToString("00") + "%");
                    saveTest(rootDir, tests.GetMember(i));
                }
                Console.SetCursorPosition(0, line);
                Console.WriteLine(url + " Complete.   ");
            }
        }

        static void Main(string[] args)
        {
            if (Directory.Exists("tests"))
            {
                Console.WriteLine("Cleaning...");
                Directory.Delete("tests", true);
            }
            var testSet = getTestSet(@"http://test262.ecmascript.org/json/default.json");
            for (var i = 0; i < TestsSource.Length; i++)
            {
                download(TestsSource[i], "tests/");
            }
        }
    }
}
