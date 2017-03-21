using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace FunctionalTests
{
    [TestClass]
    public sealed class OverloadedMethods
    {
        private class Class
        {
            public void Method()
            { }

            public void Method(int a, int b)
            { }

            public void Method(int a)
            { }
        }

        [TestMethod]
        public void OverloadedMethods_0()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(JSValue.Marshal(instance));
            context.Eval($"{nameof(instance)}.{nameof(instance.Method)}()");
        }

        [TestMethod]
        public void OverloadedMethods_1()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(JSValue.Marshal(instance));
            context.Eval($"{nameof(instance)}.{nameof(instance.Method)}(1)");
        }

        [TestMethod]
        public void OverloadedMethods_2()
        {
            var context = new Context();
            var instance = new Class();

            context.DefineVariable($"{nameof(instance)}").Assign(JSValue.Marshal(instance));
            context.Eval($"{nameof(instance)}.{nameof(instance.Method)}(1, 2)");
        }
    }
}
