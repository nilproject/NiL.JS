using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace IntegrationTests
{
    [TestClass]
    public class JSValueTests
    {
        [TestMethod]
        public void PrimitiveTypeShouldBeWrappedAsClass()
        {
            var wrappedObject = JSValue.Wrap(1);

            Assert.AreEqual(JSValueType.Object, wrappedObject.ValueType);
        }

        [TestMethod]
        public void PrimitiveTypeShouldBeMarshaledAsPrimitive()
        {
            var wrappedObject = JSValue.Marshal(1);

            Assert.AreEqual(JSValueType.Integer, wrappedObject.ValueType);
        }
    }
}
