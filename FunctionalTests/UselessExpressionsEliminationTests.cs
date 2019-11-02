using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.Core;

namespace FunctionalTests
{
    [TestClass]
    public class UselessExpressionsEliminationTests
    {
        [TestMethod]
        public void LastExpressionShouldKeepAliveInRootOfScript()
        {
            var script = Script.Parse("42");
            var context = new Context();

            var result = script.Evaluate(context);

            Assert.AreEqual(42, result.Value);
        }

        [TestMethod]
        public void LastExpressionShouldKeepAliveInRootOfEval()
        {
            var context = new Context();

            var result = context.Eval("42");

            Assert.AreEqual(42, result.Value);
        }
    }
}
