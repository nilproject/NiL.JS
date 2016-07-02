using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.Core;

namespace IntegrationTests
{
    [TestClass]
    public class ModuleTests
    {
        [TestMethod]
        public void ModuleWithEmptyCodeShouldCreateContext()
        {
            var module = new Module("");

            Assert.IsNotNull(module.Context);
        }

        [TestMethod]
        public void ExportOperatorShouldAddItemToExportTable()
        {
            var module = new Module("export var a = 0x777;");

            module.Run();

            Assert.IsNotNull(module.Exports["a"]);
            Assert.AreEqual(0x777, module.Exports["a"].Value);
        }

        [TestMethod]
        public void ImportOperatorShouldImportItem()
        {
            var module1 = new Module("");
            var privateObject = new PrivateObject(module1.Exports);
            privateObject.Invoke("set_Item", "a", JSValue.Marshal(0x777));

            var module2 = new Module("module2", "import {a} from \"another module\"");
            Module.ResolveModule += (m, e) =>
            {
                e.Module = module1;
            };

            module2.Run();

            Assert.AreEqual(0x777, module2.Context.GetVariable("a").Value);
        }
    }
}
