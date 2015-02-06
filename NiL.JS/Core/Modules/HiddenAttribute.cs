using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Член, помеченный данным аттрибутом, не будет доступен из сценария.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class HiddenAttribute : Attribute
    {
    }
}
