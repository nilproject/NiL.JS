using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.Core;

namespace Tests
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

        private sealed class DelegateModuleResolver : IModuleResolver
        {
            private readonly ModuleResolverDelegate _moduleResolverDelegate;

            public delegate bool ModuleResolverDelegate(ModuleRequest moduleRequest, out Module result);

            public DelegateModuleResolver(ModuleResolverDelegate moduleResolverDelegate)
            {
                _moduleResolverDelegate = moduleResolverDelegate ?? throw new ArgumentNullException(nameof(moduleResolverDelegate));
            }

            public bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                return _moduleResolverDelegate(moduleRequest, out result);
            }
        }

        [TestMethod]
        public void AbsolutePathShouldGenerateCorrectly()
        {
            var module0 = new Module("/src/m/main.js", @"
import '/module0.js'
import 'module1.js'
import './module2.js'
import '../module3.js'
");
            var generatedPaths = new Dictionary<string, string>();
            module0.ModuleResolversChain.Add(new DelegateModuleResolver((ModuleRequest request, out Module result) =>
            {
                result = new Module(request.AbsolutePath, "");
                generatedPaths[request.CmdArgument] = request.AbsolutePath;
                return true;
            }));

            module0.Run();

            Assert.AreEqual("/module0.js", generatedPaths["/module0.js"]);
            Assert.AreEqual("/src/m/module1.js", generatedPaths["module1.js"]);
            Assert.AreEqual("/src/m/module2.js", generatedPaths["./module2.js"]);
            Assert.AreEqual("/src/module3.js", generatedPaths["../module3.js"]);
        }

        [TestMethod]
        public void ImportOperatorShouldImportItem()
        {
            var module1 = new Module("");
            var itemProperty = module1.Exports.GetType().GetProperty("Item");
            itemProperty.SetValue(module1.Exports, JSValue.Marshal(0x777), new object[] { "a" });

            var module2 = new Module("module2", "import {a} from \"another module\"");
            module2.ModuleResolversChain.Add(new DelegateModuleResolver((ModuleRequest request, out Module result) =>
            {
                if (request.CmdArgument != "another module")
                {
                    result = null;
                    return false;
                }

                result = module1;
                return true;
            }));

            module2.Run();

            Assert.AreEqual(0x777, module2.Context.GetVariable("a").Value);
        }

        [TestMethod]
        [Timeout(2000)]
        public void DynamicImportOperatorShouldImportItem()
        {
            var module1 = new Module("");
            var itemProperty = module1.Exports.GetType().GetProperty("Item");
            itemProperty.SetValue(module1.Exports, JSValue.Marshal(0x777), new object[] { "a" });

            var module2 = new Module("module2", "var m = null; import(\"another module\").then(x => m = x)");
            module2.ModuleResolversChain.Add(new DelegateModuleResolver((ModuleRequest request, out Module result) =>
            {
                if (request.CmdArgument != "another module")
                {
                    result = null;
                    return false;
                }

                result = module1;
                return true;
            }));

            module2.Run();

            for (; ; )
            {
                Thread.Sleep(1);
                var imported = module2.Context.GetVariable("m");
                if (!imported.Defined)
                    continue;

                if (Equals(imported["a"].Value, 0x777))
                    return;
            }
        }

        [TestMethod]
        [Timeout(2000)]
        public void DynamicImportOperatorShouldImportDefaultItem()
        {
            var module1 = new Module("");
            var itemProperty = module1.Exports.GetType().GetProperty("Item");
            itemProperty.SetValue(module1.Exports, JSValue.Marshal(0x777), new object[] { string.Empty });

            var module2 = new Module("module2", "var m = null; import(\"another module\").then(x => m = x)");
            module2.ModuleResolversChain.Add(new DelegateModuleResolver((ModuleRequest request, out Module result) =>
            {
                if (request.CmdArgument != "another module")
                {
                    result = null;
                    return false;
                }

                result = module1;
                return true;
            }));

            module2.Run();

            for (; ; )
            {
                Thread.Sleep(1);
                var imported = module2.Context.GetVariable("m");
                if (!imported.Defined)
                    continue;

                if (Equals(imported["default"].Value, 0x777))
                    return;
            }
        }

        [TestMethod]
        [Timeout(2000)]
        public void ExecutionWithTimeout()
        {
            var module = new Module("for(;;)");

            var stopWatch = Stopwatch.StartNew();
            try
            {
                module.Run(1000);
            }
            catch (TimeoutException)
            {

            }
            stopWatch.Stop();

            Assert.AreEqual(1, Math.Round(stopWatch.Elapsed.TotalSeconds));
        }

        [TestMethod]
        [Timeout(2000)]
        public void ExecutionWithTimeout_ExceptionShouldNotBeCaughtByTryCatch()
        {
            var module = new Module("try{for(;;)}catch(e){throw'No, this is another exception';}");

            var stopWatch = Stopwatch.StartNew();
            try
            {
                module.Run(1000);
            }
            catch (TimeoutException)
            {

            }
            stopWatch.Stop();

            Assert.AreEqual(1, Math.Round(stopWatch.Elapsed.TotalSeconds));
        }
    }
}
