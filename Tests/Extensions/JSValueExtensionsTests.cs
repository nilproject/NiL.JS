using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Extensions;

[TestClass]
public class JSValueExtensionsTests
{
    public static IEnumerable<object[]> TestData
    {
        get
        {
            yield return new object[] { (JSValue)true, true };
            yield return new object[] { (JSValue)1, 1 };
            yield return new object[] { (JSValue)"qwe", "qwe" };
            yield return new object[] { (JSValue)1.2, 1.2 };
        }
    }

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void ConvertJsValueToClrObjectShouldWork(JSValue value, object expected) => Assert.AreEqual(expected, value.As<object>());
}
