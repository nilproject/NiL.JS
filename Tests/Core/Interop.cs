using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace Tests.Core
{
    [TestClass]
    public class Interop
    {
        public interface ISum
        {
            [JavaScriptName("sum")]
            double Sum(double a, double b);
        }

        [TestMethod]
        public void ImplementationOfInterface()
        {
            var context = new Context();

            var test = context.Eval("({ sum(a,b) { return a + b } })").AsImplementationOf<ISum>(context);
            var value = test.Sum(1, 2);

            Assert.AreEqual(3, value);
        }

        public abstract class SomeClass
        {
            [JavaScriptName("foo")]
            protected abstract int Foo();

            [JavaScriptName("boo")]
            public virtual int Boo() { return 0; }

            public int PublicFoo() => Foo();
        }

        [TestMethod]
        public void ImplementationOfClass()
        {
            var context = new Context();

            var test = context.Eval("({ foo() { return 1 }, boo() { return 2 } })").AsImplementationOf<SomeClass>(context);

            var value0 = test.PublicFoo();
            var value1 = test.Boo();

            Assert.AreEqual(1, value0);
            Assert.AreEqual(2, value1);
        }
    }
}
