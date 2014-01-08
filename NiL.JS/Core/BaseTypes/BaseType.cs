using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NiL.JS.Modules;

namespace NiL.JS.Core.BaseTypes
{
    internal abstract class BaseType : JSObject
    {
        [Hidden]
        internal JSObject proxy;
        [Hidden]
        private bool immutable;

        protected BaseType()
        {
            immutable = GetType().GetCustomAttribute(typeof(ImmutableAttribute)) != null;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            return proxy ?? (proxy = TypeProxy.Proxy(this)).GetField(name, fast, own);
        }
    }
}
