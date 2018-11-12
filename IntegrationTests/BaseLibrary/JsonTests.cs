namespace IntegrationTests.BaseLibrary
{
    using System;
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NiL.JS.BaseLibrary;
    using NiL.JS.Core;

    using Array = NiL.JS.BaseLibrary.Array;

    [TestClass]
    public sealed class JsonTests
    {
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
            var obj = JSObject.CreateObject();
            obj.DefineProperty("test").Assign(123);
            obj.DefineProperty("array").Assign(new Array(new List<JSValue> { JSValue.Marshal(123), JSValue.Marshal("test") }));
            var nestedObj = JSObject.CreateObject();
            nestedObj.DefineProperty("nil").Assign("JS!");
            nestedObj.DefineProperty("null").Assign(JSValue.Null);
            obj.DefineProperty("nested").Assign(nestedObj);

            var expected3 = new[] { "{", "   \"test\": 123,", "   \"array\": [", "      123,", "      \"test\"", "   ],", "   \"nested\": {", "      \"nil\": \"JS!\"", "   }", "}" };
            Assert.AreEqual(string.Join(Environment.NewLine, expected3), JSON.stringify(new Arguments { obj, null, 3 }));

            var expected2 = new[] { "{", "  \"test\": 123,", "  \"array\": [", "    123,", "    \"test\"", "  ],", "  \"nested\": {", "    \"nil\": \"JS!\"", "  }", "}" };
            Assert.AreEqual(string.Join(Environment.NewLine, expected2), JSON.stringify(new Arguments { obj, null, 2 }));
        }

        [TestMethod]
        [Timeout(1000)]
        public void Max10SpacesWhenStringifying()
        {
            var obj = JSObject.CreateObject();
            obj.DefineProperty("test").Assign(123);
            obj.DefineProperty("array").Assign(new Array(new List<JSValue> { JSValue.Marshal(123), JSValue.Marshal("test") }));

            var expected3 = new[] { "{", "          \"test\": 123,", "          \"array\": [", "                    123,", "                    \"test\"", "          ]", "}" };
            Assert.AreEqual(string.Join(Environment.NewLine, expected3), JSON.stringify(new Arguments { obj, null, 15 }));
        }
    }
}
