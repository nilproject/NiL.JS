using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Interop
{
    internal sealed class PrototypeProxy : Proxy
    {
        internal override bool IsInstancePrototype
        {
            get
            {
                return true;
            }
        }

        public PrototypeProxy(BaseContext context, Type type, JSObject prototype)
            : base(context, type)
        {
            __prototype = prototype;
        }
    }
}
