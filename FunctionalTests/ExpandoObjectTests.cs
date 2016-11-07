using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.Core;
using System.Collections.Generic;
using System.Dynamic;

namespace FunctionalTests
{
    [TestClass]
    public class ExpandoObjectTests
    {
        [TestMethod]
        public void ReadPropertiesOfDynamicobject()
        {
            dynamic obj = new ExpandoObject();
            obj.field = "value";
            var context = new Context();
            context.DefineVariable("obj").Assign(JSValue.Marshal(obj));

            var value = context.Eval("obj.field");

            Assert.AreEqual(JSValueType.String, value.ValueType);
            Assert.AreEqual("value", value.Value);
        }

        [TestMethod]
        public void WritePropertiesOfDynamicobject()
        {
            dynamic obj = new ExpandoObject();
            var context = new Context();
            context.DefineVariable("obj").Assign(JSValue.Marshal(obj));

            var value = context.Eval("obj.field = 'value'");

            Assert.IsInstanceOfType(obj.field, typeof(string));
            Assert.AreEqual("value", obj.field);
        }

        [TestMethod]
        public void WritePropertiesOfDynamicobjectOverWith()
        {
            dynamic obj = new ExpandoObject();
            obj.field = null;
            var context = new Context();
            context.DefineVariable("obj").Assign(JSValue.Marshal(obj));

            var value = context.Eval("with(obj) field = 'value'");

            Assert.IsInstanceOfType(obj.field, typeof(string));
            Assert.AreEqual("value", obj.field);
        }

        [TestMethod]
        public void WriteInsideWithoShouldNotCreateNewField()
        {
            dynamic obj = new ExpandoObject();
            var context = new Context();
            context.DefineVariable("obj").Assign(JSValue.Marshal(obj));

            var value = context.Eval("with(obj) field = 'value'");

            Assert.IsFalse((obj as IDictionary<string, object>).ContainsKey("field"));
        }
    }
}
