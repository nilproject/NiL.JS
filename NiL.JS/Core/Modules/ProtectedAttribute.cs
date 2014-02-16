using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Значение поля, помеченного данным аттрибутом, будет неизменяемо для скрипта.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class ProtectedAttribute : Attribute
    {

    }
}
