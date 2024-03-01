using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Fuzz;

[TestClass]
public sealed class Bug_299
{
    [TestMethod]
    public async Task Promise_299()
    {
        var script = Script.Parse(
            @"
export default class Test {
    async run(params) {
        return params;
    }
}
"
        );
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var ctor = module.Exports.Default.As<Function>();
        var instance = ctor.Construct(new Arguments());
        var run = instance.GetProperty("run").As<Function>();

        var param = JSON.parse("{}");
        var result = run.MakeDelegate<Func<JSValue, JSValue>>()(param);
        if (result.Value is Promise promise)
        {
            result = await promise.Task;
        }

        Assert.AreEqual(param, result);
    }
}
