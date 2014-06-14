using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Значение поля, помеченного данным аттрибутом, будет неизменяемо для скрипта.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ReadOnlyAttribute : Attribute
    {

    }
}
