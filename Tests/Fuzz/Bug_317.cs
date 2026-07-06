using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Fuzz;

[TestClass]
public sealed class Bug_317
{
    [TestMethod]
    public void Test1()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i =>
                              {
                                  await Task.Yield();
                                  var result = run.Call(null, new Arguments() { new { value = "b" } });

                                  return await ((Promise)result.Value).Task;
                              });

        var task = Task.WhenAll(tasks);


        var values = task.GetAwaiter().GetResult();
        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    [TestMethod]
    public async Task Test1bis()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
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


        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }



    [TestMethod]
    public async Task Test1WithAwaitInJs()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    await void 1;
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i =>
                              {
                                  await Task.Yield();
                                  var result = run.Call(null, new Arguments() { new { value = "b" } });

                                  return await ((Promise)result.Value).Task;
                              });

        var task = Task.WhenAll(tasks);


        var values = task.GetAwaiter().GetResult();
        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    [TestMethod]
    public async Task Test1WithEval()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    eval()
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i =>
                              {
                                  await Task.Yield();
                                  var result = run.Call(null, new Arguments() { new { value = "b" } });

                                  return await ((Promise)result.Value).Task;
                              });

        var task = Task.WhenAll(tasks);


        var values = task.GetAwaiter().GetResult();
        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }

    [TestMethod]
    public async Task Test1WithDebug()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext()
        {
            Debugging = true
        };
        context.DebuggerCallback += delegate { };
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i =>
                              {
                                  await Task.Yield();
                                  var result = run.Call(null, new Arguments() { new { value = "b" } });

                                  return await ((Promise)result.Value).Task;
                              });

        var task = Task.WhenAll(tasks);


        var values = task.GetAwaiter().GetResult();
        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }


    [TestMethod]
    public async Task Test1NoYield()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        var tasks = Enumerable.Range(0, 10)
                              .Select(async i =>
                              {
                                  //await Task.Yield();
                                  var result = run.Call(null, new Arguments() { new { value = "b" } });

                                  return await ((Promise)result.Value).Task;
                              });

        var task = Task.WhenAll(tasks);


        var values = task.GetAwaiter().GetResult();
        Assert.AreEqual("b", values[0].As<string>());
        Assert.AreEqual("b", values[1].As<string>());
    }


    [TestMethod]
    public async Task Test2()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        for (int i = 0; i < 10; i++)
        {
            await Task.Yield();

            var result = run.Call(null, new Arguments { new { value = "b" } });
            var value = await ((Promise)result.Value).Task;
            Assert.AreEqual("b", value.As<string>());
        }
    }


    [TestMethod]
    public async Task Test3()
    {
        var script = Script.Parse(@"
export default async function(param1) {
    return param1?.value
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var run = module.Exports.Default.As<Function>();

        for (int i = 0; i < 10; i++)
        {
            await Task.Run(async () =>
            {
                await Task.Yield();

                var result = run.Call(null, new Arguments { new { value = "b" } });
                var value = await ((Promise)result.Value).Task;
                Assert.AreEqual("b", value.As<string>());
            });
        }
    }
}
