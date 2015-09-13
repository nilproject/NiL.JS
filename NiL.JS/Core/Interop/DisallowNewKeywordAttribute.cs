using System;

namespace NiL.JS.Core.Interop
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class DisallowNewKeywordAttribute : Attribute
    {
    }
}
