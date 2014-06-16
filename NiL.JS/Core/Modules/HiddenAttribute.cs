using System;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Член, помеченный данным уттрибутом, не будет доступен из скрипта.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class HiddenAttribute : Attribute
    {
    }
}
