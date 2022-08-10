using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Fuzz
{
    /// <see cref="https://github.com/nilproject/NiL.JS/issues/268"/>
    [TestClass]
    public class TypescriptServices
    {
        private static readonly string _typescriptServicesPath = Environment.CurrentDirectory + "/../../../../TestSets/typescriptServices.js";

        private Module _module;
        private StringBuilder _output;
        private TextWriter _oldOutput;
        private TextWriter _oldErrOutput;
        private GlobalContext _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = new GlobalContext();
            _context.ActivateInCurrentThread();

            try
            {
                using (var file = new FileStream(_typescriptServicesPath, FileMode.Open, FileAccess.Read))
                using (var fileReader = new StreamReader(file))
                {
                    _module = new Module(fileReader.ReadToEnd());
                }

                _module.Context.DefineVariable("window").Assign(_module.Context.ThisBind);
                _module.Run();
            }
            catch
            {
                _context.Deactivate();
                throw;
            }

            _output = new StringBuilder();
            _oldErrOutput = Console.Error;
            _oldOutput = Console.Out;
            Console.SetOut(new StringWriter(_output));
            Console.SetError(new StringWriter(_output));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Context.CurrentContext == _context)
            {
                _context.Deactivate();
            }

            Console.SetOut(_oldOutput);
            Console.SetError(_oldErrOutput);
        }

        [TestMethod]
        public void ShouldProduceCorrectResult()
        {
            var transpileFunction = _module.Context.Eval(@"
input => {
    const result = ts.transpileModule(input, {
        compilerOptions: {
            target: ts.ScriptTarget.ES2015,
            module: ts.ModuleKind.ES2015,
            lib: [ 'ES2015' ],
            downlevelIteration: true,
        }
    });
    return result.outputText;
};
");

            var tsc = transpileFunction.As<Function>().MakeDelegate<Func<string, string>>();

            var typescriptSource = @"
import { SampleStore } from 'utiliread';
export default class Test {
    static inject = [SampleStore];
    constructor(private ss: SampleStore) {
    }
}";
            var transpiled = tsc(typescriptSource);

            Assert.AreEqual(@"import { SampleStore } from 'utiliread';
export default class Test {
    constructor(ss) {
        this.ss = ss;
    }
}
Test.inject = [SampleStore];
",
transpiled);
        }
    }
}
