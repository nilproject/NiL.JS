using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Extensions;

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

        [TestMethod]
        public void GlobalObjectsAssignMode_Allow()
        {
            var globalContext = new GlobalContext();
            globalContext.GlobalObjectsAssignMode = GlobalObjectsAssignMode.Allow;

            var context0 = new Context(globalContext);
            var context1 = new Context(globalContext);

            Assert.IsTrue(context0.Eval("Math = undefined; Math").IsUndefined());
            Assert.IsTrue(context1.Eval("Math").IsUndefined());
        }

        [TestMethod]
        public void GlobalObjectsAssignMode_ScriptLocal()
        {
            var globalContext = new GlobalContext();
            globalContext.GlobalObjectsAssignMode = GlobalObjectsAssignMode.ScriptLocal;

            var context0 = new Context(globalContext);
            var context1 = new Context(globalContext);

            Assert.IsTrue(context0.Eval("Math = undefined; Math").IsUndefined());
            Assert.IsFalse(context1.Eval("Math").IsUndefined());
        }

        [TestMethod]
        public void GlobalObjectsAssignMode_Deny()
        {
            var globalContext = new GlobalContext();
            globalContext.GlobalObjectsAssignMode = GlobalObjectsAssignMode.Deny;

            var context0 = new Context(globalContext);
            var context1 = new Context(globalContext);

            Assert.IsFalse(context0.Eval("Math = undefined; Math").IsUndefined());
            Assert.IsFalse(context1.Eval("Math").IsUndefined());
        }

        [TestMethod]
        public void GlobalObjectsAssignMode_DenyThrowException()
        {
            var globalContext = new GlobalContext();
            globalContext.GlobalObjectsAssignMode = GlobalObjectsAssignMode.DenyThrowException;

            var context0 = new Context(globalContext);
            var context1 = new Context(globalContext);

            Assert.ThrowsException<JSException>(() => context0.Eval("Math = undefined"));

            Assert.IsFalse(context0.Eval("Math").IsUndefined());
            Assert.IsFalse(context1.Eval("Math").IsUndefined());
        }
    }
}
