using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace FunctionalTests
{
    [TestClass]
    public class AsyncFunctionTests
    {
        [TestMethod]
        public async Task MarshalResolvedPromiseAsTask()
        {
            var context = new Context();

            var result = await context.Eval("function getPromise() { return new Promise((a, b) => { a('test'); }); } async function test() { return await getPromise(); } test();").As<Promise>().Task;

            Assert.AreEqual("test", result.ToString());
        }

        [TestMethod]
        public async Task MarshalRejectedPromiseAsFailedTask()
        {
            var context = new Context();

            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await context.Eval("function getPromise() { return new Promise((a, b) => { b('test'); }); } async function test() { return await getPromise(); } test();").As<Promise>().Task;
            });
        }
    }
}
