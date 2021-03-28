using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace IntegrationTests
{
    [TestClass]
    public class AsyncFunctionTests
    {
        [TestMethod]
        public async Task ResolvedPromiseShouldBeReturnedAsCompletedTask()
        {
            var context = new Context();

            context.DefineVariable("testAwaitable").Assign(JSValue.Marshal(new Func<string, Task<string>>((input) =>
            {
                var task = new Task<string>(() =>
                {
                    return input;
                });

                task.Start();

                return task;
            })));

            context.Eval("async function testAsync() { return await testAwaitable('test'); }");

            var result = await context.GetVariable("testAsync").As<Function>().Call(new Arguments()).As<Promise>().Task;

            Assert.AreEqual("test", result.ToString());
        }

        [TestMethod]
        public async Task RejectedPromiseShouldBeReturnedAsFaultedTask()
        {
            var context = new Context();

            context.DefineVariable("testAwaitable").Assign(JSValue.Marshal(new Func<string, Task<string>>(async (input) =>
            {
                await Task.Delay(500);

                throw new Exception();
            })));

            context.Eval("let result = null; async function testAsync() { result = await testAwaitable('test'); }");

            await Assert.ThrowsExceptionAsync<AggregateException>(async () =>
            {
                var result = await context.GetVariable("testAsync").As<Function>().Call(new Arguments()).As<Promise>().Task;
            });
        }

        [TestMethod]
        public async Task AsyncMethodShouldReturnValue()
        {
            // Arrange
            var request = "value";

            var script = $@"
async function script() {{
    async function demo() {{
        let request = '{request}';
        let response = await fetch(request);
        check(response);
        return response;
    }}

    let result = await demo();
    check(result);
    return result;
}}
";
            var context = new Context();

            context
                .DefineVariable("fetch")
                .Assign(JSValue.Marshal(new Func<string, Task<string>>(FetchAsync)));

            context
                .DefineVariable("check")
                .Assign(JSValue.Marshal(new Action<string>((value) => { Assert.AreEqual(request, value); })));

            context.Eval(script);

            // Act
            var result = await context.GetVariable("script")
                .As<Function>()
                .Call(new Arguments())
                .As<Promise>()
                .Task;

            // Assert
            Assert.AreEqual(request, result.Value);
        }

        private async Task<string> FetchAsync(string jsRequest)
        {
            await Task.Delay(10);
            return jsRequest;
        }
    }
}
