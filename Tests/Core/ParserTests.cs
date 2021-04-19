using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ValidateShouldNotUpdateIndexIfReturnedFalse()
        {
            var index = 0;
            var text = "1234";

            var result = Parser.Validate(text, "12", ref index);

            Assert.IsFalse(result);
            Assert.AreEqual(0, index);
        }
    }
}
