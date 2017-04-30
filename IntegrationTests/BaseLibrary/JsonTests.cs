using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace IntegrationTests.BaseLibrary
{
    [TestClass]
    public sealed class JsonTests
    {
        [TestMethod]
        [ExpectedException(typeof(JSException))]
        [Timeout(1000)]
        public void IncorrectJsonShouldBringToError()
        {
            string json = "\"a\":0";

            NiL.JS.BaseLibrary.JSON.parse(json);

            Assert.Fail();
        }
    }
}
