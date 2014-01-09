using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Modules
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class HiddenAttribute : Attribute
    {
    }
}
