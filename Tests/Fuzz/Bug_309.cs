using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Fuzz;

[TestClass]
public class Bug_309
{
    [TestMethod]
    public void SetField91()
    {
        var globalContext = new GlobalContext();
        try
        {
            globalContext.ActivateInCurrentThread();
            var context = new Context();

            context.GlobalContext.IndexersSupport = IndexersSupport.ForceEnable;
            _ = context.GlobalContext.GetConstructor(typeof(Dictionary<string, string>));
            context.GlobalContext.IndexersSupport = IndexersSupport.WithAttributeOnly;

            context.Add("test", new Dictionary<string, string> { ["myKey"] = "myValue" });

            Assert.AreEqual("myValue", context.Eval("test.myKey").ToString());
        }
        finally
        {
            globalContext.Deactivate();
        }
    }
}
