using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core;

public sealed partial class NumberUtilsTests
{
    [TestMethod]
    public void Number_prototype_toFixed_Should_Work_Correctly()
    {
        var context = new Context();
        Assert.AreEqual("0", context.Eval("0..toFixed()").ToString());
        Assert.AreEqual("1", context.Eval("1..toFixed()").ToString());
        Assert.AreEqual("1.00000", context.Eval("(1).toFixed(5)").ToString());
        Assert.AreEqual("2.0", context.Eval("(1.99).toFixed(1)").ToString());
        Assert.AreEqual("53.8", context.Eval("(53.84615384615385).toFixed(1)").ToString());
        Assert.AreEqual("1.2", context.Eval("(1.249).toFixed(1)").ToString());
        Assert.AreEqual("0.0", context.Eval("(0.046).toFixed(1)").ToString());
        Assert.AreEqual("0.00", context.Eval("(0.0046).toFixed(2)").ToString());
        Assert.AreEqual("1", context.Eval("(0.5).toFixed(0)").ToString());
        Assert.AreEqual("3", context.Eval("(2.5).toFixed(0)").ToString());
        Assert.AreEqual("1.00", context.Eval("(1.005).toFixed(2)").ToString());
        Assert.AreEqual("1.100", context.Eval("(1.100).toFixed(3)").ToString());
        Assert.AreEqual("1.01", context.Eval("(1.00500000000000001).toFixed(2)").ToString());
        Assert.AreEqual("1.11", context.Eval("1.106.toFixed(2)").ToString());
    }
}
