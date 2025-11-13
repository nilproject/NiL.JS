using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Core.Functions;

[TestClass]
public class BindedFunctionTest
{
    [TestMethod]
    public void BindedFunctionCallWithNullArgumentsShouldWork()
    {
        var context = new Context();
        var bindedFunction = context.Eval("(function(a){ return a })").As<Function>().bind(new() { JSObject.CreateObject(), "hello" });

        Assert.AreEqual(bindedFunction.Call(null).ToString(), "hello");
    }
}
