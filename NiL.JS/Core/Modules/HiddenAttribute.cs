using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.Modules
{
    /// <summary>
    /// Член, помеченный данным уттрибутом, не будет доступен из скрипта.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Constructor)]
    public sealed class HiddenAttribute : Attribute
    {
    }
}
