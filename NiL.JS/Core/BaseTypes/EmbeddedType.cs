using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    public abstract class EmbeddedType : JSObject
    {
        [Hidden]
        private bool immutable;
        [Hidden]
        private JSObject constructor;

        protected EmbeddedType()
        {
            immutable = GetType().GetCustomAttributes(typeof(ImmutableAttribute), true).Length != 0;
            oValue = this;
        }

        public virtual JSObject toString()
        {
            return ToString();
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            switch (name)
            {
                case "constructor":
                    {
                        if (constructor != null)
                            return constructor;
                        if (immutable)
                            constructor = TypeProxy.GetConstructor(this.GetType());
                        else
                        {
                            constructor = new JSObject();
                            constructor.Assign(TypeProxy.GetConstructor(this.GetType()));
                        }
                        return constructor;
                    }
                case "__proto__":
                    {
                        if (prototype != null)
                            return prototype;
                        if (immutable)
                            prototype = TypeProxy.GetPrototype(this.GetType());
                        else
                        {
                            prototype = new JSObject();
                            prototype.Assign(TypeProxy.GetPrototype(this.GetType()));
                        }
                        return prototype;
                    }
            }
            if (prototype == null)
            {
                if (immutable)
                    prototype = TypeProxy.GetPrototype(this.GetType());
                else
                {
                    prototype = new JSObject();
                    prototype.Assign(TypeProxy.GetPrototype(this.GetType()));
                }
            }
            return DefaultFieldGetter(name, fast, false);
        }

        public JSObject valueOf()
        {
            return this;
        }
    }
}
