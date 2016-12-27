using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Interop
{
    [Prototype(typeof(JSObject), true)]
    internal sealed class PrototypeProxy : Proxy
    {
        internal override bool IsInstancePrototype
        {
            get
            {
                return true;
            }
        }

        public PrototypeProxy(GlobalContext context, Type type, bool indexersSupport)
            : base(context, type, indexersSupport)
        {
        }
    }
}
