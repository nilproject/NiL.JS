using System;

namespace NiL.JS.Core.Modules
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class DisallowNewKeywordAttribute : Attribute
    {
    }
}
