using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.Core;

namespace Tests.Generated
{
    public abstract class FileTestBase
    {
        public sealed class MyTestModuleResolver : CachedModuleResolverBase
        {
            public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                var module = new Module(moduleRequest.CmdArgument, File.ReadAllText(moduleRequest.AbsolutePath));
                result = module;
                return true;
            }
        }

        private string _sta;

        protected void LoadSta(string path)
        {
            using (var f = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(f))
                _sta = sr.ReadToEnd();
        }

        protected void RunFile(string fileName)
        {
            string code;
            using (var f = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(f))
                code = sr.ReadToEnd();

            var globalContext = new GlobalContext();

            var output = new StringBuilder();
            var oldOutput = Console.Out;
            Console.SetOut(new StringWriter(output));

            try
            {
                globalContext.ActivateInCurrentThread();

                var pass = true;
                Module module;
                if (!string.IsNullOrEmpty(_sta))
                {
                    module = new Module(fileName, _sta);
                    module.Run();
                }
                else
                {
                    module = new Module(fileName, "");
                }

                module.ModuleResolversChain.Add(new MyTestModuleResolver());

                var preambleEndTemp = 0;
                int preambleEnd;

                do
                {
                    preambleEnd = preambleEndTemp;
                    try
                    {
                        preambleEndTemp = Parser.SkipComment(code, preambleEndTemp, true);
                    }
                    catch
                    {
                        break;
                    }
                }
                while (preambleEnd < preambleEndTemp);

                var negative = code.IndexOf("* @negative", 0, preambleEnd) != -1;
                var strict = code.IndexOf("* @onlyStrict", 0, preambleEnd) != -1;

                var context = module.Context;

                try
                {
                    try
                    {
                        var script = Script.Parse(code);
                        script.Evaluate(script.Root.Strict ? new Context(context, script.Root.Strict) : context);
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
                        output.Append(e);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debugger.Break();
                    output.Append(e);
                    pass = false;
                }

                Assert.IsTrue(pass, output.ToString());
                Assert.AreEqual(string.Empty, output.ToString().Trim());
            }
            finally
            {
                globalContext.Deactivate();
                Console.SetOut(oldOutput);
            }
        }
    }
}
