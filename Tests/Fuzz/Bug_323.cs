using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core;

public sealed partial class NumberUtilsTests
{
    [TestMethod]
    public void String_Of_Number_Should_Work_Correctly()
    {
        var context = new Context();
        Assert.AreEqual("1.2345", context.Eval("String(1.2345)").ToString());
        Assert.AreEqual("0.30000000000000004", context.Eval("String(0.1 + 0.2)").ToString());
        Assert.AreEqual("1.9999999999999998", context.Eval("String(1.9999999999999998)").ToString());
        //Assert.AreEqual("1.7976931348623157e+308", context.Eval("String(Number.MAX_VALUE)").ToString());
        //Assert.AreEqual("5e-324", context.Eval("String(5e-324)").ToString());
    }
}
