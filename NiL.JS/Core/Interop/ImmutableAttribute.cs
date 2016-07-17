using System;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Объект-прослойка, созданный для типа, помеченного данным аттрибутом, 
    /// не будет допускать создание полей, которые не существуют в помеченном типе.
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class ImmutableAttribute : Attribute
    {
    }
}
