using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests
{
    [TestClass]
    public sealed class OverloadedMethods
    {
        private class Class
        {
            public int Method()
            {
                return 0;
            }

            public int Method(int[] a)
            {
                return 3;
            }

            public int Method(int a, int b)
            {
                return 2;
            }

            public int Method(int a)
            {
                return 1;
            }
        }

        [TestMethod]
        public void OverloadedMethods_0()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(Context.CurrentGlobalContext.ProxyValue(instance));
            var result = context.Eval($"{nameof(instance)}.{nameof(instance.Method)}()");

            Assert.AreEqual(0, result.Value);
        }

        [TestMethod]
        public void OverloadedMethods_1()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(context.GlobalContext.ProxyValue(instance));
            var result = context.Eval($"{nameof(instance)}.{nameof(instance.Method)}(1)");

            Assert.AreEqual(1, result.Value);
        }

        [TestMethod]
        public void OverloadedMethods_2()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(Context.CurrentGlobalContext.ProxyValue(instance));
            var result = context.Eval($"{nameof(instance)}.{nameof(instance.Method)}(1, 2)");

            Assert.AreEqual(2, result.Value);
        }

        [TestMethod]
        public void OverloadedMethods_3()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(Context.CurrentGlobalContext.ProxyValue(instance));
            var result = context.Eval($"{nameof(instance)}.{nameof(instance.Method)}([1, 2])");

            Assert.AreEqual(3, result.Value);
        }
    }
}
