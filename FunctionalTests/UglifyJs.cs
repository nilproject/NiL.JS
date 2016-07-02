using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;

namespace FunctionalTests
{
    [TestClass]
    public class UglifyJs
    {
        private static readonly string UglifyJsScriptPath = Environment.CurrentDirectory + "/../../../Tests/uglifyjs.js";

        private Module _module;

        [TestInitialize]
        public void Initialize()
        {
            using (var file = new FileStream(UglifyJsScriptPath, FileMode.Open))
            using (var fileReader = new StreamReader(file))
                _module = new Module(fileReader.ReadToEnd());
            _module.Run();
        }

        [TestMethod]
        public void UglifyJsShouldWorkCorrectly()
        {
            var myString =
@"(function (fallback) {
    fallback = fallback || function () { };
})(null);";
            _module.Context.DefineVariable("code").Assign(myString);

            var result = _module.Context.Eval(
@"var ast = UglifyJS.parse(code);
ast.figure_out_scope();
compressor = UglifyJS.Compressor();
ast = ast.transform(compressor);
ast.print_to_string();");

            Assert.AreEqual("!function(fallback){fallback=fallback||function(){}}(null);", result.ToString());
        }
    }
}
