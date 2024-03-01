using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests;

[TestClass]
public sealed class ToStringTagTests
{
    [TestMethod]
    public void JSObjectMastHaveObjectTag()
    {
        var context = new Context();
        var code = @"new Object().toString()";

        var stringValue = context.Eval(code);

        Assert.AreEqual("[object Object]", stringValue.Value);
    }

    [ToStringTag("Tag")]
    private sealed class CustomObjectWithTag : JSObject
    {
        public CustomObjectWithTag()
        {
            ValueType = JSValueType.Object;
            Value = this;
        }
    }

    [TestMethod]
    public void ToStringTagShouldWorkForCustomObjects()
    {
        var context = new Context();
        context.DefineVariable("test").Assign(new CustomObjectWithTag());
        var code = @"test.toString()";

        var stringValue = context.Eval(code);

        Assert.AreEqual("[object Tag]", stringValue.Value);
    }

    private sealed class CustomObjectWithoutTag : JSObject
    {
        public CustomObjectWithoutTag()
        {
            ValueType = JSValueType.Object;
            Value = this;
        }
    }

    [TestMethod]
    public void ToStringShouldUseNameOfCustomObjectIfToStringTagNotDefined()
    {
        var context = new Context();
        context.DefineVariable("test").Assign(new CustomObjectWithoutTag());
        var code = @"test.toString()";

        var stringValue = context.Eval(code);

        Assert.AreEqual($"[object {typeof(CustomObjectWithoutTag).Name}]", stringValue.Value);
    }

    [ToStringTag("Tag")]
    private sealed class ExternTypeWithTag
    {
    }

    [TestMethod]
    public void ToStringTagShouldWorkForExternTypes()
    {
        var context = new Context();
        context.DefineVariable("test").Assign(new ExternTypeWithTag(), context);
        var code = @"test.toString()";

        var stringValue = context.Eval(code);

        Assert.AreEqual("[object Tag]", stringValue.Value);
    }

    private sealed class ExternTypeWithoutTag
    {
    }

    [TestMethod]
    public void ToStringShouldUseNameOfTypeIfToStringTagNotDefined()
    {
        var context = new Context();
        context.DefineVariable("test").Assign(new ExternTypeWithoutTag(), context);
        var code = @"test.toString()";

        var stringValue = context.Eval(code);

        Assert.AreEqual($"[object {typeof(ExternTypeWithoutTag).Name}]", stringValue.Value);
    }

    [TestMethod]
    public void ToStringTagShouldBeSymbol()
    {
        var context = new Context();
        context.DefineVariable("test").Assign(new ExternTypeWithTag(), context);
        var code = @"test[Symbol.toStringTag]";

        var stringValue = context.Eval(code);

        Assert.AreEqual("Tag", stringValue.Value);
    }
}
