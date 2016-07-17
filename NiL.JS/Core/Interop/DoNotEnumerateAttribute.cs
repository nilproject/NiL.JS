using System;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Указывает, что помеченный член следует пропустить при перечислении в конструкции for-in
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class DoNotEnumerateAttribute : Attribute
    {

    }
}
