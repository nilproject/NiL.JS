using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Expressions;

[TestClass]
public class AwaitTests
{
    [TestMethod]
    public void NoAwaitAsyncFunctionShouldNotThrow()
    {
        var script = Script.Parse(@"
async function noAwait(){
    var a = 1;
}
export default async function() {
    await noAwait();
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();
        var result = run.Call(new Arguments());

        if (result.Value is not Promise promise)
            Assert.Fail();
        else
            promise.Task.GetAwaiter().GetResult();
    }

    [TestMethod]
    public void EmptyAsyncFunctionShouldReturnPromise()
    {
        var script = Script.Parse(@"
export default async function() {
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();
        var result = run.Call(new Arguments());

        if (result.Value is not Promise promise)
            Assert.Fail();
        else
            promise.Task.GetAwaiter().GetResult();
    }

    [TestMethod]
    public void EmptyAsyncFunctionShouldNotThrow()
    {
        var script = Script.Parse(@"
async function noAwait(){
}
export default async function() {
    await noAwait();
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();
        var result = run.Call(new Arguments());

        if (result.Value is not Promise promise)
            Assert.Fail();
        else
            promise.Task.GetAwaiter().GetResult();
    }

    [TestMethod]
    public void AsyncFunctionReturningPromiseShouldReturnValue()
    {
        var script = Script.Parse(@"
async function returnsPromise(){
    return Promise.resolve(7);
}
export default async function() {
    return await returnsPromise();
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();
        var result = run.Call(new Arguments());

        Assert.IsTrue(result.Value is Promise);
        var resolved = ((Promise)result.Value).Task.GetAwaiter().GetResult();
        Assert.AreEqual(7, resolved.As<int>());
    }
}
