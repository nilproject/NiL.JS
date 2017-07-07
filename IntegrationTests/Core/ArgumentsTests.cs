using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;

namespace IntegrationTests.Core
{
    [TestClass]
    public class ArgumentsTests
    {
        [TestMethod]
        public void ValuesEnumeration()
        {
            Assert.AreEqual(1, new Arguments { Number.POSITIVE_INFINITY }.Count());
            Assert.AreEqual(0, new Arguments { }.Count());
        }
    }
}
