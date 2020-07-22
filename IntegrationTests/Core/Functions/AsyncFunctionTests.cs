using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace IntegrationTests
{
    [TestClass]
    public class AsyncFunctionTests
    {
        [TestMethod]
        public async Task Echo()
        {
            var asyncModule = new Module("/async-module.js", "");

            asyncModule.Context.DefineVariable("test").Assign(JSValue.Marshal(new Func<string, Task<string>>(async (input) =>
            {
                await Task.Delay(500);

                return input;
            })));

            asyncModule.Context.Eval("export { test };");

            var entryModule = new Module("/entry-module.js", "import * as am from 'async-module'; let result = null; async function entryTest() { result = await am.test('test'); }");
            var resolver = new _ModuleResolver();

            resolver.Modules.Add(asyncModule);

            entryModule.ModuleResolversChain.Clear();
            entryModule.ModuleResolversChain.Add(resolver);

            entryModule.Run();

            var args = new Arguments();

            args[0] = JSValue.Marshal("test");

            var fn = entryModule.Context.GetVariable("entryTest").As<Function>();

            await fn.Call(args).As<Promise>().Task;

            var result = entryModule.Context.GetVariable("result").ToString();

            Assert.AreEqual("test", result);
        }

        [TestMethod]
        public async Task ExceptionsBubbleUpToCaller()
        {
            var asyncModule = new Module("/async-module.js", "");

            asyncModule.Context.DefineVariable("test").Assign(JSValue.Marshal(new Func<string, Task<string>>(async (input) =>
            {
                await Task.Delay(500);

                throw new Exception("test");
            })));

            asyncModule.Context.Eval("export { test };");

            var entryModule = new Module("/entry-module.js", "import * as am from 'async-module'; let result = null; async function entryTest() { result = await am.test('test'); }");
            var resolver = new _ModuleResolver();

            resolver.Modules.Add(asyncModule);

            entryModule.ModuleResolversChain.Clear();
            entryModule.ModuleResolversChain.Add(resolver);

            entryModule.Run();

            var args = new Arguments();

            args[0] = JSValue.Marshal("test");

            await Assert.ThrowsExceptionAsync<AggregateException>(async () =>
            {
                var fn = entryModule.Context.GetVariable("entryTest").As<Function>();

                await fn.Call(args).As<Promise>().Task;
            });
        }

        private class _ModuleResolver : IModuleResolver
        {
            public List<Module> Modules { get; } = new List<Module>();

            public bool TryGetModule(ModuleRequest moduleRequest, out Module result)
            {
                result = Modules.FirstOrDefault(m => m.FilePath == moduleRequest.AbsolutePath);

                return (result != null);
            }
        }
    }
}
