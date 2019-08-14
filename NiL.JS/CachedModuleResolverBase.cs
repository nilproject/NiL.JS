using NiL.JS.Core;

namespace NiL.JS
{
    public abstract class CachedModuleResolverBase : IModuleResolver
    {
        private static StringMap<Module> _modulesCache = new StringMap<Module>();

        bool IModuleResolver.TryGetModule(ModuleRequest moduleRequest, out Module result)
        {
            if (_modulesCache.TryGetValue(moduleRequest.AbsolutePath, out result))
                return true;

            if (TryGetModule(moduleRequest, out result))
            {
                _modulesCache.Add(moduleRequest.AbsolutePath, result);
                return true;
            }

            return false;
        }

        public abstract bool TryGetModule(ModuleRequest moduleRequest, out Module result);
    }
}
