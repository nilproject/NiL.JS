using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core;

[TestClass]
public class JSValueTests
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
    public void PrimitiveTypeShouldBeWrappedAsClass()
    {
        var wrappedObject = Context.CurrentGlobalContext.WrapValue(1);

        Assert.AreEqual(JSValueType.Object, wrappedObject.ValueType);
    }

    [TestMethod]
    public void PrimitiveTypeShouldBeMarshaledAsPrimitive()
    {
        var wrappedObject = Context.CurrentGlobalContext.ProxyValue(1);

        Assert.AreEqual(JSValueType.Integer, wrappedObject.ValueType);
    }

    private enum TestEnum : byte
    {
        First = 0,
        // Missing = 1,
        Second = 2,
        Another = 3,
        YetAnother = 4
    }

    [TestMethod]
    public void ShouldConvertStringToEnumValue()
    {
        var convertableJsString = Context.CurrentGlobalContext.ProxyValue(nameof(TestEnum.Second)) as IConvertible;

        var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

        Assert.AreEqual(TestEnum.Second, enumValue);
    }

    [TestMethod]
    public void ShouldConvertNumberInStringToEnumValue()
    {
        var convertableJsString = Context.CurrentGlobalContext.ProxyValue(((int)(TestEnum.Second)).ToString()) as IConvertible;

        var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

        Assert.AreEqual(TestEnum.Second, enumValue);
    }

    [TestMethod]
    public void ShouldConvertIntToEnumValue()
    {
        var convertableJsString = Context.CurrentGlobalContext.ProxyValue((int)(TestEnum.Second)) as IConvertible;

        var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

        Assert.AreEqual(TestEnum.Second, enumValue);
    }

    [TestMethod]
    public void ShouldReturnNullIfCannotConvert()
    {
        var convertableJsString = Context.CurrentGlobalContext.ProxyValue("bazinga!") as IConvertible;

        var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

        Assert.AreEqual(null, enumValue);
    }
}
