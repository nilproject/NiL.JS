using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.Interop;
using NiL.JS.Extensions;

namespace NiL.JS.BaseLibrary;


[RequireNewKeyword]

public sealed class ShadowRealm
{
    private readonly Module _mod;

    internal ShadowRealm(IModuleResolver[] allowedModules)
    {
        _mod = new Module("", "");
        _mod.Context.DefineVariable("globalThis").Assign(_mod.Context.ThisBind);
        foreach (var moduleResolver in allowedModules)
        {
            _mod.ModuleResolversChain.Add(moduleResolver);
        }
    }



    public JSValue evaluate(Arguments a)
    {
        var str = a[0].As<string>();
        return _mod.Context.Eval(str);
        
    }

    public JSValue importValue(Arguments a)
    {
        var path = a[0].As<string>();
        var name = a[1].As<string>();
        var imp = _mod.Import(path);
        var promise =
            new Promise(Task.FromResult<JSValue>(name == "default" ? imp.Exports.Default : imp.Exports[name]));
        return _mod.Context.GlobalContext.ProxyValue(promise);
    }
}