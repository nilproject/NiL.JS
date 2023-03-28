using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace Tests.Fuzz
{
    [TestClass]
    public class Indexers
    {
        [UseIndexers]
        public sealed class TestClass
        {
            public List<string> Values { get; } = new();

            public string this[string i]
            {
                get { return i; }
                set { Values.Add(i + value); }
            }
        }

        [TestMethod]
        public void IndexersFuzz()
        {
            var context = new Context();
            var jsval = Context.CurrentGlobalContext.ProxyValue(new TestClass());
            context.DefineVariable("test").Assign(jsval);
            context.Eval(
                "var prot = test.__proto__;" +
                "test.__proto__ = null;" +
                "test.__proto__ = prot;" +
                "test.prop2 = 2;" +
                "Object.setPrototypeOf(test, { __proto__: prot, prop: Object });" +
                "test.prop = 3;" +
                "test.prop3 = 4;");

            Assert.IsTrue((bool)jsval.hasOwnProperty(new Arguments { "__proto__" }));
            Assert.IsTrue((bool)jsval.hasOwnProperty(new Arguments { "prop" }));
            Assert.AreEqual(3, jsval["prop"].As<int>());
            Assert.IsTrue((bool)jsval.hasOwnProperty(new Arguments { "prop2" }));
            Assert.AreEqual(2, jsval["prop2"].As<int>());
            Assert.AreNotEqual(jsval["__proto__"], jsval.__proto__);
            CollectionAssert.Contains(jsval.As<TestClass>().Values, "prop34");
            Assert.AreEqual(1, jsval.As<TestClass>().Values.Count);
        }
    }
}
