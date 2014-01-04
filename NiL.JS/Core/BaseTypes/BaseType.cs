using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Modules;

namespace NiL.JS.Core.BaseTypes
{

    internal class BaseType : JSObject
    {
        [Hidden]
        internal ClassProxy proxy;

        public virtual JSObject toString()
        {
            return ToString();
        }

        public virtual JSObject valueOf()
        {
            return this;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            return (proxy ?? (proxy = new ClassProxy(this) { prototype = prototype })).GetField(name, fast);
        }
    }
}
