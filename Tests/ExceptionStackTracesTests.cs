using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests
{
    [TestClass]
    public class ExceptionStackTracesTests
    {
        [TestMethod]
        public void ShouldIncludeJsMethods()
        {
            var context = new Context();

            try
            {
                context.Eval(@"function jsExceptionMethod() { throw 'Hello, world!'; } jsExceptionMethod()");
            }
            catch (Exception e)
            {
                var stackTrace = e.StackTrace;

                Assert.IsTrue(stackTrace.Contains("jsExceptionMethod"));
            }
        }


        [TestMethod]
        public void ShouldProcessCodeTypeSwitch()
        {
            var context = new Context();

            context.DefineVariable("complexEval").Assign(new Func<string, JSValue>(x => new Context().Eval(x)));

            try
            {
                context.Eval(
@"function jsExceptionMethod() {
   complexEval(""[1].map(x=> { throw 'oops!' });"")
}
jsExceptionMethod()");
            }
            catch (Exception e)
            {
                var stackTrace = e.StackTrace;

                Assert.IsTrue(stackTrace.Contains("jsExceptionMethod"), stackTrace);
                Assert.IsTrue(stackTrace.Contains("Array.map"), stackTrace);
            }
        }
    }
}
