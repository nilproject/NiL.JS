using System;

namespace NiL.JS.Core.Modules
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal sealed class FieldAttribute : Attribute
    {
    }
}
