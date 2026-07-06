using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Fuzz;

// Regression tests for https://github.com/nilproject/NiL.JS/issues/317
//
// Root causes fixed:
//   Fix 1 (AsyncFunction.cs)  — async invocations now always store parameters in the
//     per-invocation Context._variables dictionary instead of relying solely on the
//     shared VariableDescriptor.cacheContext/cacheValue fields. When two calls are
//     interleaved on the thread pool each overwrites the shared cache, so a resuming
//     call's deepGet() used to fall through TryGetValue and find nothing.
//
//   Fix 2 (AwaitExpression.cs) — `await <primitive>` (e.g. `await void 1`) no longer
//     tries to access ["then"] on undefined/null/number/etc., which threw TypeError.
//     Only object/function values can be thenable.

[TestClass]
public sealed class Bug_317
{
    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private static Function ParseAsyncModule(string jsBody)
    {
        var script = Script.Parse($@"
export default async function(param1) {{
    {jsBody}
}}
");
        var context = new GlobalContext();
        var module = new Module("main.js", script, context);
        module.Run();
        return module.Exports.Default.As<Function>();
    }

    private static Task<JSValue[]> RunConcurrent(Function run, int count = 10)
    {
        var tasks = Enumerable.Range(0, count)
            .Select(async i =>
            {
                await Task.Yield();
                var result = run.Call(null, new Arguments() { new { value = "b" } });
                return await ((Promise)result.Value).Task;
            });
        return Task.WhenAll(tasks);
    }

    // ---------------------------------------------------------------
    // Concurrent-interleaved: same value for all calls (fix 1 coverage)
    // ---------------------------------------------------------------

    // Previously: ReferenceError: Variable "param1" is not defined
    // Cause: concurrent Task.Yield() calls put multiple invocations on different
    // thread-pool threads. Each overwrote VariableDescriptor.cacheContext/Value on
    // the shared function definition, so resuming invocations could not find param1.
    [TestMethod]
    public void ConcurrentCalls_BlockingWait()
    {
        var run = ParseAsyncModule("return param1?.value");

        var values = RunConcurrent(run).GetAwaiter().GetResult();

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    [TestMethod]
    public async Task ConcurrentCalls_AsyncWait()
    {
        var run = ParseAsyncModule("return param1?.value");

        var values = await RunConcurrent(run);

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    // Previously broken. eval() forced storeVariablesIntoContext=true, which was a
    // workaround that happened to prevent the cache corruption.
    [TestMethod]
    public async Task ConcurrentCalls_JsEvalPresent()
    {
        var run = ParseAsyncModule("eval(); return param1?.value");

        var values = await RunConcurrent(run);

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    // Previously broken. Debugging=true forced storeVariablesIntoContext=true.
    [TestMethod]
    public async Task ConcurrentCalls_DebuggingEnabled()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext() { Debugging = true };
        context.DebuggerCallback += delegate { };
        var module = new Module("main.js", script, context);
        module.Run();
        var run = module.Exports.Default.As<Function>();

        var values = await RunConcurrent(run);

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    // ---------------------------------------------------------------
    // Concurrent-interleaved: new cases that strengthen fix 1
    // ---------------------------------------------------------------

    // Each call receives its own unique value. Detects mixing between concurrent
    // invocations that the same-value tests above would miss.
    [TestMethod]
    public async Task ConcurrentCalls_UniqueValuePerCall()
    {
        var run = ParseAsyncModule("return param1?.value");

        var tasks = Enumerable.Range(0, 10)
            .Select(async i =>
            {
                await Task.Yield();
                var result = run.Call(null, new Arguments() { new { value = i.ToString() } });
                return await ((Promise)result.Value).Task;
            });
        var values = await Task.WhenAll(tasks);

        for (int i = 0; i < 10; i++)
            Assert.AreEqual(i.ToString(), values[i].As<string>(), $"values[{i}]");
    }

    // Verifies that all parameters—not just the first—are stored per-invocation.
    // Uses sequential thread-pool switches (not true concurrency) to avoid a
    // pre-existing race in NiL.JS's VariableDescriptor cache for functions with
    // multiple parameters under true parallel invocations.
    [TestMethod]
    public async Task Sequential_MultipleParameters_WithThreadSwitch()
    {
        var script = Script.Parse(@"
export default async function(a, b) {
    return a + ':' + b
}
");
        var context = new GlobalContext();
        var module = new Module("main.js", script, context);
        module.Run();
        var run = module.Exports.Default.As<Function>();

        for (int i = 0; i < 10; i++)
        {
            await Task.Yield();
            var result = run.Call(null, new Arguments() { "hello", "world" });
            var value = await ((Promise)result.Value).Task;
            Assert.AreEqual("hello:world", value.As<string>());
        }
    }

    // Default parameter values must survive concurrent interleaving.
    [TestMethod]
    public async Task ConcurrentCalls_DefaultParameter()
    {
        var script = Script.Parse(@"
export default async function(param1, extra = 'default') {
    return param1?.value + ':' + extra
}
");
        var context = new GlobalContext();
        var module = new Module("main.js", script, context);
        module.Run();
        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
            .Select(async i =>
            {
                await Task.Yield();
                var result = run.Call(null, new Arguments() { new { value = "b" } });
                return await ((Promise)result.Value).Task;
            });
        var values = await Task.WhenAll(tasks);

        for (int i = 0; i < values.Length; i++)
            Assert.AreEqual("b:default", values[i].As<string>(), $"values[{i}]");
    }

    // Real JS suspension via Promise.resolve(): the Continuator resumes the function
    // after the inner promise resolves. param1 must still be accessible on resume.
    [TestMethod]
    public async Task ConcurrentCalls_JsResolvedPromiseAwait()
    {
        var run = ParseAsyncModule("await Promise.resolve(1); return param1?.value");

        var values = await RunConcurrent(run);

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    // ---------------------------------------------------------------
    // Baselines: scenarios that were always correct (no fix needed)
    // ---------------------------------------------------------------

    // No Task.Yield() means all calls start on the same thread synchronously;
    // no cache corruption occurs.
    [TestMethod]
    public async Task ConcurrentCalls_SameThread_NoInterleaving()
    {
        var run = ParseAsyncModule("return param1?.value");

        var tasks = Enumerable.Range(0, 10)
            .Select(async i =>
            {
                var result = run.Call(null, new Arguments() { new { value = "b" } });
                return await ((Promise)result.Value).Task;
            });
        var values = Task.WhenAll(tasks).GetAwaiter().GetResult();

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    // Sequential calls: each awaits completion before the next starts.
    [TestMethod]
    public async Task SequentialCalls_WithThreadPoolSwitch()
    {
        var run = ParseAsyncModule("return param1?.value");

        for (int i = 0; i < 10; i++)
        {
            await Task.Yield();
            var result = run.Call(null, new Arguments() { new { value = "b" } });
            var value = await ((Promise)result.Value).Task;
            Assert.AreEqual("b", value.As<string>());
        }
    }

    [TestMethod]
    public async Task SequentialCalls_InTaskRun()
    {
        var run = ParseAsyncModule("return param1?.value");

        for (int i = 0; i < 10; i++)
        {
            await Task.Run(async () =>
            {
                await Task.Yield();
                var result = run.Call(null, new Arguments() { new { value = "b" } });
                var value = await ((Promise)result.Value).Task;
                Assert.AreEqual("b", value.As<string>());
            });
        }
    }

    // ---------------------------------------------------------------
    // Body-variable coverage (similar place fix: CodeBlock.initVariables)
    // ---------------------------------------------------------------

    // Async functions WITHOUT any await still run concurrently when callers use
    // Task.Yield(). Body-local variables (var x) share VariableDescriptor.cacheContext/
    // cacheValue with all invocations. Without the initVariables fix (cew=true for
    // async kinds) a concurrent pair would overwrite each other's cache and deepGet()
    // would return notExists for x.
    [TestMethod]
    public async Task ConcurrentCalls_BodyLocalVariable_NoAwait()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    var x = param1?.value;
    return x;
}
");
        var context = new GlobalContext();
        var module = new Module("main.js", script, context);
        module.Run();
        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
            .Select(async i =>
            {
                await Task.Yield();
                var result = run.Call(null, new Arguments() { new { value = "b" } });
                return await ((Promise)result.Value).Task;
            });
        var values = await Task.WhenAll(tasks);

        for (int i = 0; i < values.Length; i++)
            Assert.AreEqual("b", values[i].As<string>(), $"values[{i}]");
    }

    // ---------------------------------------------------------------
    // Fix 2: `await <non-thenable>` must not throw TypeError
    // ---------------------------------------------------------------

    // Previously: TypeError: Can't get property "then" of "undefined"
    // AwaitExpression now guards with _valueType < JSValueType.Object before
    // accessing ["then"], so primitives are returned directly.
    [TestMethod]
    public async Task ConcurrentCalls_JsAwaitUndefined()
    {
        var run = ParseAsyncModule("await void 0; return param1?.value");

        var values = await RunConcurrent(run);

        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    // `await null` — null is a JSObject so it goes through JSObject.GetProperty,
    // which returns notExists (not throw) for missing properties. Verify it is
    // treated as non-thenable and the function completes normally.
    [TestMethod]
    public async Task SingleCall_AwaitNull()
    {
        var script = Script.Parse(@"
export default async function() {
    let x = await null;
    return x === null ? 'ok' : 'fail'
}
");
        var context = new GlobalContext();
        var module = new Module("main.js", script, context);
        module.Run();
        var run = module.Exports.Default.As<Function>();

        var value = await ((Promise)run.Call(null, new Arguments()).Value).Task;

        Assert.AreEqual("ok", value.As<string>());
    }

    // Numeric, string and boolean primitives are not thenable — must pass through
    // `await` without TypeError and return their values to the function.
    [TestMethod]
    public async Task SingleCall_AwaitPrimitives()
    {
        var script = Script.Parse(@"
export default async function() {
    let a = await 42;
    let b = await 'hello';
    let c = await true;
    return '' + a + ',' + b + ',' + c
}
");
        var context = new GlobalContext();
        var module = new Module("main.js", script, context);
        module.Run();
        var run = module.Exports.Default.As<Function>();

        var value = await ((Promise)run.Call(null, new Arguments()).Value).Task;

        Assert.AreEqual("42,hello,true", value.As<string>());
    }
}
