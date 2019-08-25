using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Interop
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class InliningAttribute : Attribute
    {
    }
}
