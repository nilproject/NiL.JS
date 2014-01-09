using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NiL.JS.Modules;

namespace NiL.JS.Core.BaseTypes
{
    internal abstract class EmbeddedType : JSObject
    {
        [Hidden]
        private bool immutable;
        [Hidden]
        private JSObject constructor;

        protected EmbeddedType()
        {
            immutable = GetType().GetCustomAttributes(typeof(ImmutableAttribute), true).Length != 0;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            switch (name)
            {
                case "constructor":
                    {
                        if (constructor != null)
                            return constructor;
                        constructor = new JSObject();
                        constructor.Assign(TypeProxy.GetConstructor(this.GetType()));
                        return constructor;
                    }
                case "__proto__":
                    {
                        if (prototype != null)
                            return prototype;
                        prototype = new JSObject();
                        prototype.Assign(TypeProxy.GetPrototype(this.GetType()));
                        return prototype;
                    }
            }
            if (prototype == null)
            {
                prototype = new JSObject();
                prototype.Assign(TypeProxy.GetPrototype(this.GetType()));
            }
            return prototype.GetField(name, fast || immutable, own);
        }
    }
}
