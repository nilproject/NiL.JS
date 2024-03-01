using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace Tests.Expressions;

[TestClass]
public class EqualTests
{
    [TestMethod]
    public void ComparisonWithNotExistsShouldWorkProperly()
    {
        Action f = () => { };
        var context = new Context();
        context.DefineVariable("test").Assign(f);

        var result = context.Eval("test() == 'hi there'");

        Assert.IsFalse((bool)result);
    }
}