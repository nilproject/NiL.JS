using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.BaseLibrary
{
    [TestClass]
    public class StringTests
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
        public void ReplaceWithRegexpAndReplacer()
        {
            var script = @"
function foo() { 
    var e = 'ab'; 
    var i = e.replace(/[a-z]/g, f => String.fromCharCode((f <= 'Z' ? 90 : 122) >= (f = (f.charCodeAt(0)) % 26 + (f <= 'Z' ? 90 : 122) - 26) ? f : f - 26)); 
    return i;
}

foo();";
            var context = new Context();
            var result = context.Eval(script);

            Assert.AreEqual(JSValueType.String, result.ValueType);
            Assert.AreEqual("st", result.Value);
        }

        [TestMethod]
        public void StringIterator()
        {
            var value = "12345678";
            var str = new NiL.JS.BaseLibrary.String(value);

            var iterator = str.iterator();
            var index = 0;
            for (var item = iterator.next(); !item.done; item = iterator.next(), index++)
            {
                Assert.AreEqual(value[index].ToString(), item.value.ToString());
            }
        }

        [TestMethod]
        public void StringIteratorInCode()
        {
            var context = new Context();
            var script = NiL.JS.Script.Parse(@"let x = 0;
for (var codePoint of 'foo') {
  x++;
}
x;");
            var value = script.Evaluate(context);
            Assert.AreEqual(3, value);
        }
    }
}
