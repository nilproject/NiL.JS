using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests
{
    [TestClass]
    public class AsyncFunctionTests
    {
        [TestMethod]
        public void CompletedTaskShouldBeReturnedAsResolvedPromise()
        {
            var context = new Context();

            context.DefineVariable("testAwaitable").Assign(new Func<string, Task<string>>((input) =>
            {
                return Task.FromResult(input);
            }), context);

            context.Eval("function testAsync() { return testAwaitable('test'); }");

            var task = context.GetVariable("testAsync").As<Function>().Call(new Arguments()).As<Promise>().Task;

            Assert.AreEqual("test", task.GetAwaiter().GetResult().ToString());
        }

        [TestMethod]
        public async Task RejectedPromiseShouldBeReturnedAsFaultedTask()
        {
            var context = new Context();

            context.DefineVariable("testAwaitable").Assign(context.GlobalContext.ProxyValue(new Func<string, Task<string>>(async (input) =>
            {
                await Task.Delay(500);

                throw new Exception();
            })));

            context.Eval("let result = null; async function testAsync() { result = await testAwaitable('test'); }");

            var task = context.GetVariable("testAsync").As<Function>().Call(new Arguments()).As<Promise>().Task;
            await Assert.ThrowsExceptionAsync<AggregateException>(async () =>
            {
                await task;
            });
        }

        [TestMethod]
        public void AsyncFunctionWithNestedLiteralContextsShouldWorkAsExpected()
        {
            var script = Script.Parse(@"
async function test() {
    const a = await Promise.resolve(123);
    if (true) {
        const b = await Promise.resolve(456);
        return b + a;
    }
}
");
            var context = new Context();
            script.Evaluate(context);

            var promiseValue = context.Eval("test()");
            var promise = promiseValue.Value as Promise;

            Assert.AreEqual(579, promise.Task.GetAwaiter().GetResult());
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
                .Assign(new Func<string, Task<string>>(FetchAsync), context);

            context
                .DefineVariable("check")
                .Assign(new Action<string>((value) => { Assert.AreEqual(request, value); }), context);

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
