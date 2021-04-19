using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;

namespace Tests.Core
{
    [TestClass]
    public class JSValueTests
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

        private enum TestEnum : byte
        {
            First = 0,
            // Missing = 1,
            Second = 2,
            Another = 3,
            YetAnother = 4
        }

        [TestMethod]
        public void ShouldConvertStringToEnumValue()
        {
            var convertableJsString = JSValue.Marshal(nameof(TestEnum.Second)) as IConvertible;

            var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

            Assert.AreEqual(TestEnum.Second, enumValue);
        }

        [TestMethod]
        public void ShouldConvertNumberInStringToEnumValue()
        {
            var convertableJsString = JSValue.Marshal(((int)(TestEnum.Second)).ToString()) as IConvertible;

            var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

            Assert.AreEqual(TestEnum.Second, enumValue);
        }

        [TestMethod]
        public void ShouldConvertIntToEnumValue()
        {
            var convertableJsString = JSValue.Marshal((int)(TestEnum.Second)) as IConvertible;

            var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

            Assert.AreEqual(TestEnum.Second, enumValue);
        }

        [TestMethod]
        public void ShouldReturnNullIfCannotConvert()
        {
            var convertableJsString = JSValue.Marshal("bazinga!") as IConvertible;

            var enumValue = convertableJsString.ToType(typeof(TestEnum), null);

            Assert.AreEqual(null, enumValue);
        }
    }
}
