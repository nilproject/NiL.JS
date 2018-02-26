using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace FunctionalTests
{
    [TestClass]
    public sealed class StringInterpolationTests
    {
        [TestMethod]
        public void StringInterpolationAllowsStrings()
        {
            var context = new Context();
            var code = @"`This is a string`";
            var stringValue = context.Eval(code);

            Assert.AreEqual("This is a string", stringValue.Value);
        }

        [TestMethod]
        public void StringInterpolationAllowsSlashes()
        {
            var context = new Context();
            var code = @"`This is a string such as http://www.google.com`";
            var stringValue = context.Eval(code);
            Assert.AreEqual("This is a string such as http://www.google.com", stringValue.Value);
        }

        [TestMethod]
        public void StringAllowsSlashes()
        {
            var context = new Context();
            var code = @"'This is a string such as http://www.google.com'";
            var stringValue = context.Eval(code);
            Assert.AreEqual("This is a string such as http://www.google.com", stringValue.Value);
        }

        [TestMethod]
        public void StringAllowsSlashesDoubleQuoted()
        {
            var context = new Context();
            var code = @"""This is a string such as http://www.google.com""";
            var stringValue = context.Eval(code);
            Assert.AreEqual("This is a string such as http://www.google.com", stringValue.Value);
        }

        [TestMethod]
        public void StringInterpolationAllowsSubstititions()
        {
            var context = new Context();
            var code = @"var a=1234; `This is a string such as ${a}`";
            var stringValue = context.Eval(code);

            Assert.AreEqual("This is a string such as 1234", stringValue.Value);
        }
    }
}
