using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.BaseLibrary
{
    [TestClass]
    public class SetTests
    {
        [TestInitializeAttribute]
        public void TestInitialize()
        {
            new GlobalContext().ActivateInCurrentThread();
        }

        [TestCleanup]
        public void MyTestMethod()
        {
            Context.CurrentContext.GlobalContext.Deactivate();
        }

        [TestMethod]
        public void RemovesElements()
        {
            var script = @"
const s=new Set([1,2,3,4]);
s.has(2);";
            var context = new Context();
            var result = context.Eval(script);

            Assert.AreEqual(JSValueType.Boolean, result.ValueType);
            Assert.AreEqual(true, result.Value);
        }
    }
}
