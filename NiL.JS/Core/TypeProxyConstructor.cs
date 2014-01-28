using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.JS.Core
{
    internal class TypeProxyConstructor : ExternalFunction
    {
        internal readonly TypeProxy proxy;

        private static JSObject empty(Context context, JSObject args)
        {
            return null;
        }

        private TypeProxyConstructor()
            : base(empty)
        {

        }

        public TypeProxyConstructor(CallableField del, TypeProxy typeProxy)
            : base(del)
        {
            proxy = typeProxy;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            return proxy.GetField(name, fast, own);
        }
    }
}
