using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace FunctionalTests
{
    [TestClass]
    public class ParamsArraySupportTests
    {
        public sealed class ClassWithTwoMethods
        {
            public readonly List<object[]> Arguments = new List<object[]>();

            public void Method1(int prm0, params int[] prms)
            {
                Arguments.Add(new object[] { prm0, prms });
            }

            public void Method2(int prm0, int[] prms)
            {
                Arguments.Add(new object[] { prm0, prms });
            }
        }

        [TestMethod]
        public void SimpleArgumentsShouldBeWrappedIntoArray()
        {
            var testClass = new ClassWithTwoMethods();
            var context = new Context();
            context.DefineVariable("test").Assign(testClass);

            context.Eval("test.Method1(1, 2, 3, 4, 5)");

            Assert.AreEqual(1, testClass.Arguments[0][0]);
            Assert.AreEqual(2, (testClass.Arguments[0][1] as int[])[0]);
            Assert.AreEqual(3, (testClass.Arguments[0][1] as int[])[1]);
            Assert.AreEqual(4, (testClass.Arguments[0][1] as int[])[2]);
            Assert.AreEqual(5, (testClass.Arguments[0][1] as int[])[3]);
        }

        [TestMethod]
        public void OneSimpleArgumentShouldBeWrappedIntoArray()
        {
            var testClass = new ClassWithTwoMethods();
            var context = new Context();
            context.DefineVariable("test").Assign(testClass);

            context.Eval("test.Method1(1, 2)");

            Assert.AreEqual(1, testClass.Arguments[0][0]);
            Assert.AreEqual(2, (testClass.Arguments[0][1] as int[])[0]);
        }

        [TestMethod]
        public void OneArrayArgumentShouldWorkAsRaw()
        {
            var testClass = new ClassWithTwoMethods();
            var context = new Context();
            context.DefineVariable("test").Assign(testClass);

            context.Eval("test.Method1(1, [2])");

            Assert.AreEqual(1, testClass.Arguments[0][0]);
            Assert.AreEqual(2, (testClass.Arguments[0][1] as int[])[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(JSException))]
        public void TwoArrayArgumentsShouldCauseException()
        {
            var testClass = new ClassWithTwoMethods();
            var context = new Context();
            context.DefineVariable("test").Assign(testClass);

            context.Eval("test.Method1(1, [2], [3])");
        }

        [TestMethod]
        public void OneSimpleArgumentShouldNotBeWrappedIntoArray()
        {
            var testClass = new ClassWithTwoMethods();
            var context = new Context();
            context.DefineVariable("test").Assign(testClass);

            context.Eval("test.Method2(1, 2)");

            Assert.AreEqual(1, testClass.Arguments[0][0]);
            Assert.AreEqual(null, testClass.Arguments[0][1]);
        }

        [TestMethod]
        public void OneArrayArgumentShouldBeProcessedCorrectly()
        {
            var testClass = new ClassWithTwoMethods();
            var context = new Context();
            context.DefineVariable("test").Assign(testClass);

            context.Eval("test.Method2(1, [2])");

            Assert.AreEqual(1, testClass.Arguments[0][0]);
            Assert.AreEqual(2, (testClass.Arguments[0][1] as int[])[0]);
        }
    }
}
