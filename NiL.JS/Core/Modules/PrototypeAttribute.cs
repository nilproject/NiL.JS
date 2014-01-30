using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core.Modules
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
    public sealed class PrototypeAttribute : Attribute
    {
        public Type PrototypeType { get; private set; }

        public PrototypeAttribute(Type type)
        {
            PrototypeType = type;
        }
    }
}
