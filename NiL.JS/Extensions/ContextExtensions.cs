using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Core.Functions;
using Array = NiL.JS.BaseLibrary.Array;

namespace NiL.JS.Extensions
{
    public static class ContextExtensions
    {
        public static void Add(this Context context, string key, object value)
        {
            context.DefineVariable(key).Assign(context.GlobalContext.ProxyValue(value));
        }

        
        /// <summary>
        /// Add implementation of ShadowRealm API
        /// <see cref="https://github.com/tc39/proposal-shadowrealm"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="allowedResolvers"><see cref="IModuleResolver"/> that used for importValue</param>
        /// <remarks>At current moment not allowed to use Shadow Realm in Shadow realm</remarks>
        /// <returns>Context with ShadowRealm contructor</returns>
        public static Context AddShadowRealm(this Context context, IModuleResolver[] allowedResolvers = null)
        {
            //Workaround
            context.DefineVariable("globalThis").Assign(context.ThisBind);
            var resolvers = allowedResolvers ?? System.Array.Empty<IModuleResolver>();
            var del = new Func<ShadowRealm>(() => new ShadowRealm(resolvers));
            var func = context.GlobalContext.ProxyValue(del).As<Function>();
            func.RequireNewKeywordLevel = RequireNewKeywordLevel.WithNewOnly;
            context.DefineVariable("ShadowRealm").Assign(func);
            return context;
        }
        
        /// <summary>
        /// Add implementation of ShadowRealm API to module context
        /// <see cref="https://github.com/tc39/proposal-shadowrealm"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="allowedResolvers"><see cref="IModuleResolver"/> that used for importValue. If null - used from ModuleResolversChain</param>
        /// <remarks>At current moment not allowed to use Shadow Realm in Shadow realm</remarks>
        /// <returns>Context with ShadowRealm contructor</returns>
        public static Module AddShadowRealm(this Module module, IModuleResolver[] allowedResolvers = null)
        {
            AddShadowRealm(module.Context, allowedResolvers ?? module.ModuleResolversChain.ToArray());
            return module;
        }
        

        public static void Add(this Context context, string key, JSValue value)
        {
            context.DefineVariable(key).Assign(value);
        }
    }
}
