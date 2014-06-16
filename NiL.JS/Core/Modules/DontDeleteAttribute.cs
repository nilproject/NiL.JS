using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Член, помеченный данным аттрибутом, не будет удаляться оператором "delete".
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DoNotDeleteAttribute : Attribute
    {
    }
}
