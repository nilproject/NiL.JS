using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using System.Collections;
using System;
using System.Linq;

namespace Tests.Core
{
    [TestClass]
    public class ToolsTests
    {
        private static readonly Func<JSValue, Type, bool, object> ConvertJStoObj = (Func<JSValue, Type, bool, object>)typeof(Tools)
                .GetMethod("ConvertJStoObj", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<JSValue, Type, bool, object>));

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

        [TestMethod]
        public void ConvertArrayBufferToByteArrayTest()
        {
            var data = new byte[] { 1, 2, 3, 4 };

            object res = ConvertJStoObj(new ArrayBuffer(data), typeof(byte[]), true);

            Assert.IsInstanceOfType(res, typeof(byte[]));
            CollectionAssert.AreEqual(data, (byte[])res);
        }

        [TestMethod]
        public void ConvertFloat32ArrayToNativeArrayTest()
        {
            var data = new float[] { 1, 2, 3, 4 };
            var srcArray = new Float32Array(data.Length);
            srcArray.set(new Arguments { data });

            object res = ConvertJStoObj(srcArray, typeof(float[]), true);

            Assert.IsInstanceOfType(res, typeof(float[]));
            CollectionAssert.AreEqual(data, (float[])res);
        }

        [TestMethod]
        public void ConvertFloat64ArrayToNativeArrayTest()
        {
            var data = new double[] { 1, 2, 3, 4 };
            var srcArray = new Float64Array(data.Length);
            srcArray.set(new Arguments { data });

            object res = ConvertJStoObj(srcArray, typeof(double[]), true);

            Assert.IsInstanceOfType(res, typeof(double[]));
            CollectionAssert.AreEqual(data, (double[])res);
        }

        [TestMethod]
        public void ConvertJSArrayToNativeArrayTest()
        {
            var data = new string[] { "1", "2", "3", "4" };
            var srcArray = new NiL.JS.BaseLibrary.Array(data);

            object res = ConvertJStoObj(srcArray, typeof(double[]), true);

            Assert.IsInstanceOfType(res, typeof(double[]));
            CollectionAssert.AreEqual(data.Select(double.Parse).ToArray(), (double[])res);
        }
    }
}
