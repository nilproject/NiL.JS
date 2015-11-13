using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
    public sealed class SuspendableContext : Context
    {
        public Dictionary<CodeNode, object> SuspendData { get; private set; }

        public SuspendableContext(Context prototype, bool createFields, Function owner)
            : base(prototype, createFields, owner)
        {
            SuspendData = new Dictionary<CodeNode, object>();
        }
    }
}
