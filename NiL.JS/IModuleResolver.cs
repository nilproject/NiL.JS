namespace NiL.JS
{
    public interface IModuleResolver
    {
        bool TryGetModule(ModuleRequest moduleRequest, out Module result);
    }
}
