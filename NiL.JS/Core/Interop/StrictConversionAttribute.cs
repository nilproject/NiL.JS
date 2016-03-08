using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Interop
{
    [AttributeUsage(
        AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Delegate, 
        AllowMultiple = false, 
        Inherited = false)]
    public sealed class StrictConversionAttribute : Attribute
    {
    }
}
