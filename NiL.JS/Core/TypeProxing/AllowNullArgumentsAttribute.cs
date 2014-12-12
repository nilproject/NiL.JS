using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.TypeProxing
{
    /// <summary>
    /// For compatibility with legacy code.
    /// Specifies that method will work correctly if container of arguments will be equal to null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    public sealed class AllowNullArgumentsAttribute : Attribute
    {
    }
}
