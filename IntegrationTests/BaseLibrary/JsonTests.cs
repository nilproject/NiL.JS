using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace IntegrationTests.BaseLibrary
{
    using Array = NiL.JS.BaseLibrary.Array;

    [TestClass]
    public sealed class JsonTests
    {
        abstract class Base
        {
            public abstract object this[string key] { get; set; }
            public abstract Dummy this[Guid key] { get; }

        }
        private class Dummy: Base
        {
            public string Title => "title";

            public override object this[string key] { get => key; set { } }
            public override Dummy this[Guid key] => null;
        }

        [TestMethod]
        [ExpectedException(typeof(JSException))]
        [Timeout(1000)]
        public void IncorrectJsonShouldBringToError()
        {
            string json = "\"a\":0";

            JSON.parse(json);

            Assert.Fail();
        }

        [TestMethod]
        [Timeout(1000)]
        public void SpacingWhenStringifying()
        {
            lock (JSObject.Null)
            {
                var obj = JSObject.CreateObject();
                obj.DefineProperty("test").Assign(123);
                obj.DefineProperty("array").Assign(new Array { 123, "test" });
                var nestedObj = JSObject.CreateObject();
                nestedObj.DefineProperty("nil").Assign("JS!");
                nestedObj.DefineProperty("null").Assign(JSValue.Null);
                obj.DefineProperty("nested").Assign(nestedObj);

                var expected3 = string.Join(Environment.NewLine, new[] { "{", "   \"test\": 123,", "   \"array\": [", "      123,", "      \"test\"", "   ],", "   \"nested\": {", "      \"nil\": \"JS!\"", "   }", "}" });
                var stringified = JSON.stringify(new Arguments { obj, null, 3 });
                Assert.AreEqual(expected3, stringified);

                var expected2 = string.Join(Environment.NewLine, new[] { "{", "  \"test\": 123,", "  \"array\": [", "    123,", "    \"test\"", "  ],", "  \"nested\": {", "    \"nil\": \"JS!\"", "  }", "}" });
                stringified = JSON.stringify(new Arguments { obj, null, 2 });
                Assert.AreEqual(expected2, stringified);
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public void Max10SpacesWhenStringifying()
        {
            lock (JSObject.Null)
            {
                var obj = JSObject.CreateObject();
                obj.DefineProperty("test").Assign(123);
                obj.DefineProperty("array").Assign(new Array { 123, "test" });

                var expected3 = new[] { "{", "          \"test\": 123,", "          \"array\": [", "                    123,", "                    \"test\"", "          ]", "}" };
                Assert.AreEqual(string.Join(Environment.NewLine, expected3), JSON.stringify(new Arguments { obj, null, 15 }));
            }
        }

        [TestMethod]
        [Timeout(1000)]
        public void CustomSerializer()
        {
            var ctx = new GlobalContext();

            JSValue obj = JSValue.Marshal(new Dummy());

            ctx.ActivateInCurrentThread();

            try
            {
                Assert.IsNotNull(JSON.stringify(obj));
            }
            finally
            {
                ctx.Deactivate();
            }
        }
    }
}
