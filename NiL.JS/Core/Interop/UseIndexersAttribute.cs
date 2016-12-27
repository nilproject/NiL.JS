using System;

namespace NiL.JS.Core.Interop
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class UseIndexersAttribute : Attribute
    {
    }
}
