using System;

namespace NiL.JS.Core.Interop;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class ToStringTagAttribute : Attribute
{
    public string Tag { get; }

    public ToStringTagAttribute(string tag)
    {
        Tag = tag;
    }
}
