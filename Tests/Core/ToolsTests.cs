using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Collections;
using System;

namespace Tests.Core
{
    [TestClass]
    public class ToolsTests
    {
        [TestMethod]
        public void LongNumberShouldParsedCorrectly()
        {
            var numbers = new KeyValuePair<double, string>[]
            {
                new KeyValuePair<double, string>(1.42119667662395410000, "1.42119667662395410000"),
                new KeyValuePair<double, string>(26394813313751084.00000000000000000000, "26394813313751084.00000000000000000000"),
                new KeyValuePair<double, string>(16.00000000000000000000,"16.00000000000000000000"),
                new KeyValuePair<double, string>(10.41269841269841300000, "10.41269841269841300000"),
                new KeyValuePair<double, string>(0.76190476190476186000, "0.76190476190476186000"),
                new KeyValuePair<double, string>(0.00002260186576944384, "0.00002260186576944384"),
                new KeyValuePair<double, string>(15.49206349206349200000, "15.49206349206349200000"),
                new KeyValuePair<double, string>(14.22222222222222100000, "14.22222222222222100000"),
                new KeyValuePair<double, string>(13.96825396825396800000, "13.96825396825396800000"),
                new KeyValuePair<double, string>(13.20634920634920600000, "13.20634920634920600000"),
                new KeyValuePair<double, string>(8.98846567431158e+307, "8.98846567431158e+307"),
                new KeyValuePair<double, string>(0.35826121851261677000, "0.35826121851261677000"),
                new KeyValuePair<double, string>(3.29119230376073580000, "3.29119230376073580000"),
                new KeyValuePair<double, string>(1664158979.1109629, "1664158979.11096290000000000000"),
                new KeyValuePair<double, string>(0.00021140449751288852, "0.00021140449751288852"),
                new KeyValuePair<double, string>(34.970703125, "34.970703125"),
                new KeyValuePair<double, string>(1.7158203125, "1.7158203125"),
                new KeyValuePair<double, string>(0.6, "0.6")
            };

            foreach (var number in numbers)
            {
                var parsedNumber = 0.0;
                var result = Tools.ParseJsNumber(number.Value, out parsedNumber, 0);

                Assert.IsTrue(result);
                Assert.AreEqual(number.Key, parsedNumber);
            }
        }

        [TestMethod]
        public void FormatArgsTest()
        {
            Context c = new Context();

            var vals = new object[][]
            {
                // basic
                new object[] { null }, // no arguments
                new object[] { "", "" },
                new object[] { "1 2", "1", 2 },

                new object[] { "%s", "%s" },

                // numbers
                new object[] { "% 1 2 % 3", "%% %i %f %%", 1.1232, 2, 3 },
                new object[] { "0 1 02 003 0004 00005 000006", "%i %.1i %.2i %.3i %.4i %.5i %.6i", 0, 1, 2, 3.1415, 4, 5.8, 6 },
                new object[] { "0 0 0", "%i %i %i", "Infinity", "-Infinity", "NaN" },

                new object[] { "0 4 0", "%i %i %i", "abc", 4.234, Number.POSITIVE_INFINITY },
                new object[] { "0 4 0", "%i %i %i", "abc", "4.234", "Infinity" },
                new object[] { "NaN 4.234 Infinity", "%f %f %f", "abc", 4.234, Number.POSITIVE_INFINITY },
                new object[] { "NaN 4.234 Infinity", "%f %f %f", "abc", "4.234", "Infinity" },

                // %s and %o
                new object[] { "a b 5 4.567 NaN", "%s %s %s", "a", 'b', 5, 4.567, Number.NaN },

                new object[] { "2 \"a\" \"c\" NaN", "%o %o %o %o", 2, "a", 'c', Number.NaN },

                // Arrays and Objects
                new object[] { "Array (4) [ 1, \"abc\", /abc/i, Array[2] ]", c.Eval("o=[1,'abc',/abc/i,[1,2]]") },
                new object[] { "Object { a: 2, b: \"abc\", c: /abc/i, d: Array[2] }", c.Eval("o={a:2, b:'abc', c:/abc/i, d:[1,2]}") },

                new object[] { "System.Object [object Object]", "%s %s", new object(), JSObject.CreateObject() },
                new object[] { "Object {  } Object {  }", "%o %o", new object(), JSObject.CreateObject() },

                new object[] { "1,2,3,[object Math],function Object() { [native code] }", "%s", c.Eval("[1,2,[3],Math,Object]") },
                new object[] { "Array (5) [ 1, 2, Array[1], Math, Object() ]", "%o", c.Eval("[1,2,[3],Math,Object]") },

                new object[] { "[object Object]", "%s", c.Eval("o={a:2, b:'abc', c:Math, d:Object, e:[1,2]}") },
                new object[] { "Object { a: 2, b: \"abc\", c: Math, d: Object(), e: Array[2], f: /abc/i }", "%o", c.Eval("o={a:2, b:'abc', c:Math, d:Object, e:[1,2], f:new RegExp('abc', 'i')}") },

                // null and undefined
                new object[] { "null undefined", "%o", JSValue.Null, JSValue.Undefined },
                new object[] { "Object { n: null }", "%o", c.Eval("o={n:null}") },

                // Math...
                new object[] { "Math {  }", "%o", c.Eval("o=Math") },

                // %o vs %O
                new object[] { "Object { a: Object }", "%o", c.Eval("o={a:{a:{a:1}}}") },
                new object[] { "Object { a: Object { a: Object } }", "%O", c.Eval("o={a:{a:{a:1}}}") },


                // invalid format specifiers
                new object[] { "%.% %.2% %.-1i %e %. %.8e2f %.5 1 2 3", "%.% %.2% %.-1i %e %. %.8e2f %.5", 1, 2, 3 }
            };

            var FormatArgs = (Func<IEnumerable, string>)typeof(Tools).GetMethod("FormatArgs", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).CreateDelegate(typeof(Func<IEnumerable, string>));

            foreach (var val in vals)
            {
                var args = new Arguments();
                for (int i = 1; i < val.Length; i++)
                {
                    args.Add(val[i]);
                }
                var result = FormatArgs(args);

                Assert.IsTrue((result == null) == (val[0] == null));

                if (result != null)
                    Assert.AreEqual(val[0], result);
            }
        }
    }
}
