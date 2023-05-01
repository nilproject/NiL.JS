using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Tests;

[TestClass]
public class ShadowRealmTest
{
    private sealed class DelegateModuleResolver : IModuleResolver
    {
        private readonly ModuleResolverDelegate _moduleResolverDelegate;

        public delegate bool ModuleResolverDelegate(ModuleRequest moduleRequest, out Module result);

        public DelegateModuleResolver(ModuleResolverDelegate moduleResolverDelegate)
        {
            _moduleResolverDelegate = moduleResolverDelegate ??
                                      throw new ArgumentNullException(nameof(moduleResolverDelegate));
        }

        public bool TryGetModule(ModuleRequest moduleRequest, out Module result)
        {
            return _moduleResolverDelegate(moduleRequest, out result);
        }
    }


    [TestMethod]
    public void GlobalThisExist()
    {
        var ctx = new Context().AddShadowRealm();
        ctx.DefineVariable("lol").Assign(123);
        Assert.IsTrue(ctx.Eval("globalThis === this").As<bool>());
        Assert.IsTrue(ctx.Eval("globalThis.lol").As<int>() == 123);
        Assert.IsTrue(ctx.Eval("this.lol").As<int>() == 123);
    }

    [TestMethod]
    public void ShadowRealmEvaluateExpressionAndReturnNumber()
    {
        var ctx = new Context().AddShadowRealm();
        ctx.Eval(@"
const realm = new ShadowRealm()
var result = realm.evaluate(`(function () {
globalThis.lol = 123
return globalThis.lol })()`)");

        Assert.IsTrue(ctx.GetVariable("result").As<int>() == 123);
    }

    [TestMethod]
    public void ShadowRealmGlobalThisNotContextModuleThis()
    {
        var ctx = new Context().AddShadowRealm();
        ctx.Eval(@"
const realm = new ShadowRealm()
var result = globalThis === realm.evaluate(`globalThis`)");

        Assert.IsTrue(ctx.GetVariable("result").As<bool>() == false);
    }

    [TestMethod]
    public void ShadowRealmImportValueMustCallModuleResolver()
    {
        var ctx = new Context().AddShadowRealm(new[]
        {
            new DelegateModuleResolver(((ModuleRequest request, out Module result) =>
            {
                result = null;
                if (request.AbsolutePath == "/test.js")
                {
                    result = new Module("export default testing = 123");
                    return true;
                }

                return false;
            }))
        });
        ctx.Eval(@"async function test() {
const realm = new ShadowRealm()
var result = await realm.importValue('./test.js', 'default')
return result
}");


        Assert.IsTrue(
            ctx.GetVariable("test").As<Function>().Call(new Arguments()).As<Promise>().Task.Result.As<int>() == 123);
    }

    [TestMethod]
    public void ShadowRealmInModuleShouldWork()
    {
        var ctx = new Module(@"
const realm = new ShadowRealm()
export default async function() {
return await realm.importValue('./test.js', 'default')
} ").AddShadowRealm(new[]
        {
            new DelegateModuleResolver(((ModuleRequest request, out Module result) =>
            {
                result = null;
                if (request.AbsolutePath == "/test.js")
                {
                    result = new Module("export default testing = 123");
                    return true;
                }

                return false;
            }))
        });
        ctx.Run();
        
        Assert.IsTrue(ctx.Exports.Default.As<Function>().Call(new Arguments()).As<Promise>().Task.Result.As<int>() ==
                      123);
    }
}