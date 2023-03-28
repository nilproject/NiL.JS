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

namespace Tests
{
    [TestClass]
    public class FunctionsToDelegateWrapping
    {
        [TestMethod]
        public void TryToAddFunctionIntoListOfDelegates_Marshal()
        {
            var context = new Context();
            var list = new List<Func<string, string>>();
            context.DefineVariable("list").Assign(Context.CurrentGlobalContext.ProxyValue(list));

            context.Eval("list.push(x => 'hi ' + x)"); // IList marshaled as NativeList with array-like interface

            Assert.AreEqual("hi Test", list[0]("Test"));
        }

        [TestMethod]
        public void TryToAddFunctionIntoListOfDelegates_Wrap()
        {
            var context = new Context();
            var list = new List<Func<string, string>>();
            context.DefineVariable("list").Assign(Context.CurrentGlobalContext.WrapValue(list));

            context.Eval("list.Add(x => 'hi ' + x)");

            Assert.AreEqual("hi Test", list[0]("Test"));
        }

        [TestMethod]
        public async Task TryExecuteAsyncTaskFromJs()
        {
            var executionCount = 0;

            Task<bool> runCSharp()
            {
                executionCount++;
                return Task.FromResult(true);
            }

            var module = new Module(@"
                async function wrappedExecutesOnce() {
                    var res = await runCSharp();
                    return res;
                }
                async function wrappedExecutesTwice() {
                    return await runCSharp();
                }
            ");

            module
                .Context
                .DefineVariable("runCSharp")
                .Assign(module.Context.GlobalContext.ProxyValue(new Func<Task<bool>>(runCSharp)));

            module.Run();

            var result1 = await module
                .Context
                .GetVariable("wrappedExecutesOnce")
                .As<Function>()
                .Call(new Arguments())
                .As<Promise>()
                .Task;

            Assert.AreEqual(true, result1.Value);
            Assert.AreEqual(1, executionCount);

            var result2 = await module
                .Context
                .GetVariable("wrappedExecutesTwice")
                .As<Function>()
                .Call(new Arguments())
                .As<Promise>()
                .Task;

            Assert.AreEqual(true, result2.Value);
            Assert.AreEqual(2, executionCount);
        }
    }
}
