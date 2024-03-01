using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.BaseLibrary;

[TestClass]
public class SetTests
{
    [TestInitializeAttribute]
    public void TestInitialize()
    {
        new GlobalContext().ActivateInCurrentThread();
    }

    [TestCleanup]
    public void MyTestMethod()
    {
        Context.CurrentContext.GlobalContext.Deactivate();
    }

    [TestMethod]
    public void ContainsElement()
    {
        var script = @"
const s=new Set([1,2,3,4]);
s.has(2);";
        var context = new Context();
        var result = context.Eval(script);

        Assert.AreEqual(JSValueType.Boolean, result.ValueType);
        Assert.AreEqual(true, result.Value);
    }


    [TestMethod]
    public void DeletesElement()
    {
        var script = @"
const s=new Set([1,2,3,4]);
s.delete(2);";
        var context = new Context();
        var result = context.Eval(script);

        Assert.AreEqual(JSValueType.Boolean, result.ValueType);
        Assert.AreEqual(true, result.Value);
    }

    [TestMethod]
    public void DontAddElement()
    {
        var script = @"
const s = new Set([1,2,3,4]);
s.add(2).size;";
        var context = new Context();
        var result = context.Eval(script);

        Assert.AreEqual(JSValueType.Integer, result.ValueType);
        Assert.AreEqual(4, result.Value);
    }

}
