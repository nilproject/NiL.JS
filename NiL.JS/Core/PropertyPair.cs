using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    public sealed class PropertyPair
    {
        internal Function get;
        internal Function set;

        public Function Getter { get { return get; } }
        public Function Setter { get { return set; } }
    }
}
