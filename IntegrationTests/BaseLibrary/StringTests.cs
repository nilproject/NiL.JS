using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace IntegrationTests.BaseLibrary
{
    [TestClass]
    public class StringTests
    {
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
    }
}
