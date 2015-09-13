using System;

namespace NiL.JS.Core.Interop
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class FieldAttribute : Attribute
    {
    }
}
