using System;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Значение поля, помеченного данным аттрибутом, будет неизменяемо для скрипта.
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ReadOnlyAttribute : Attribute
    {

    }
}
