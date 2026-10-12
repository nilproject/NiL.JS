using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.Core;

namespace Tests.Core;

[TestClass]
public class ParserTests
{
    [TestMethod]
    public void ValidateShouldNotUpdateIndexIfReturnedFalse()
    {
        var index = 0;
        var text = "1234";

        var result = Parser.Validate(text, "12", ref index);

        Assert.IsFalse(result);
        Assert.AreEqual(0, index);
    }

    [TestMethod]
    public void ExportAsyncFunctionTest()
    {
        var code = @"
export async function a(x){
    return x + await test(x);
}";

        Script.Parse(code);
    }

    [TestMethod]
    [DataRow("var a = 'x'// comment\nreturn a;", "x")]
    [DataRow("var a = \"x\"// comment\nreturn a;", "x")]
    [DataRow("var a = 'x'/* comment */\nreturn a;", "x")]
    [DataRow("var a = 'x'// a / b\nreturn a;", "x")]
    [DataRow("var a = '1'// '2' ? 'x'.replace(/\\D/g, '') : 0;\nreturn a;", "1")]
    [DataRow("var a = 'x'// comment\nvar b = 4 / 2;// comment\nreturn a + b;", "x2")]
    public void CommentRightAfterStringLiteral(string code, string expected)
    {
        var context = new Context();

        var result = context.Eval("(function() {\n" + code + "\n})()");

        Assert.AreEqual(expected, result.ToString());
    }
}
