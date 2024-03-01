using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests.Fuzz;

[TestClass]
public sealed class Bug_239
{
    [TestMethod]
    public void IterationOfNativeList()
    {
        var script = Script.Parse(@"
export default class Test {
    run(ints) {
        var result = 0;
        for (const i of ints) {
            result += i;
        }
        return result;
    }
}
");
        var context = new GlobalContext();
        var module = new Module($"main.js", script, context);
        module.Run();

        var ctor = module.Exports.Default.As<Function>();
        var instance = ctor.Construct(new Arguments());
        var run = instance.GetProperty("run").As<Function>();

        var ints = new int[] { 1, 2, 3 };
        var result = run.MakeDelegate<Func<int[], JSValue>>()(ints);

        Assert.AreEqual(6, result);
    }
}
