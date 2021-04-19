using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core
{
    [TestClass]
    public class ContextTests
    {
        [TestMethod]
        public void Eval_ThisBindShouldBeOverridable()
        {
            var context = new Context();
            var oldThis = context.ThisBind;
            var myThis = JSObject.CreateObject();

            var value0 = context.Eval("this", myThis);
            var value1 = context.Eval("this");

            Assert.AreEqual(myThis, value0);
            Assert.AreNotEqual(null, value1);
            Assert.AreEqual(oldThis, value1);
        }

        [TestMethod]
        public void ContextInNonStrictModeByDefault()
        {
            var context = new Context();

            var isStrict = context.Eval(@"
try { 
    (function(){ return arguments.callee; })();
    true;
}
catch(e) {
    false;
}");

            Assert.AreEqual(true, (bool)isStrict);
        }

        [TestMethod]
        public void ContextCanBeInStrictMode()
        {
            var context = new Context(true);

            var isStrict = context.Eval(@"
try { 
    (function(){ return arguments.callee; })();
    false;
}
catch(e) {
    true;
}");

            Assert.AreEqual(true, (bool)isStrict);
        }
    }
}
