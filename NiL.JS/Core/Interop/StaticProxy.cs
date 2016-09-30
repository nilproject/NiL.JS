using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core.Interop
{
    internal sealed class StaticProxy : Proxy
    {
        internal override JSObject prototypeInstance
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

        public StaticProxy(GlobalContext context, Type type)
            : base(context, type)
        {

        }
    }
}
