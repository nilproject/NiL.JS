using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace IntegrationTests.Core.Functions
{
    [TestClass]
    public class MethodProxyTests
    {
        public class MyObject
        {
            public string Text { get; set; }
        }

        [TestMethod]
        public void UndefinedToStringPropertyShouldConvertToNull()
        {
            var context = new Context();
            var obj = new MyObject();
            context.DefineVariable("obj").Assign(JSValue.Wrap(obj));

            context.Eval("obj.Text = undefined");

            Assert.IsNull(obj.Text);
        }
    }
}
