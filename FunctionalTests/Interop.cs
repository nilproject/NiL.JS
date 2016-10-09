using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace FunctionalTests
{
    [TestClass]
    public class Interop
    {
        [TestMethod]
        public void CreateInstanceOfGenericType()
        {
            var context = new Context();
            context.DefineConstructor(typeof(List<>));

            var result = context.Eval("new (List(Number))()").Value;

            Assert.AreSame(typeof(List<Number>), result.GetType());
        }
    }
}
