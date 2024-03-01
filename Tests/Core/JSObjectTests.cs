using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core;

[TestClass]
public sealed class JSObjectTests
{
    [TestMethod]
    public void EnumerationShouldNotIncludePropertiesFromPrototype()
    {
        var proto = JSObject.CreateObject();
        proto["B"] = 1;
        var obj = JSObject.CreateObject();
        obj["A"] = 2;
        obj.__proto__ = proto;

        var L = obj.ToArray();

        Assert.AreEqual(1, L.Length);
        Assert.AreEqual(L[0].Key, "A");
        Assert.AreEqual(L[0].Value.Value, 2);
    }
}
