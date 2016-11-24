using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Interop
{
    [Prototype(typeof(JSObject), true)]
    internal sealed class StaticProxy : Proxy
    {
        internal override JSObject PrototypeInstance
        {
            get
            {
                return null;
            }
        }

        internal override bool IsInstancePrototype
        {
            get
            {
                return false;
            }
        }

        [Hidden]
        public StaticProxy(GlobalContext context, Type type, bool indexersSupport)
            : base(context, type, indexersSupport)
        {

        }
    }
}
