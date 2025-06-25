using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System.Linq;

namespace Tests.Fuzz;

[TestClass]
public class Bug_306
{
    private class MyJsValue : JSValue
    {
        public MyJsValue()
        {
            ValueType = JSValueType.Object;
            Value = this;
        }

        protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope propertyScope) => key;
    }

    [TestMethod]
    public void JsValueInheritance()
    {
        var value = new MyJsValue();
        var context = new Context() { { "value", value } };


        var prop = context.Eval("value.someProp");

        Assert.IsNotNull(prop);
        Assert.IsNotNull(prop.Value);
        Assert.AreEqual("someProp", prop.Value);
    }
}
