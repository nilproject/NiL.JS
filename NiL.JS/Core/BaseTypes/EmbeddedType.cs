using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.BaseTypes
{
    public abstract class EmbeddedType : JSObject
    {
        protected EmbeddedType()
        {
            prototype = TypeProxy.GetPrototype(this.GetType());
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
                case "__proto__":
                    {
                        if (prototype != null)
                            return prototype;
                        prototype = TypeProxy.GetPrototype(this.GetType());
                        return prototype;
                    }
            }
            return DefaultFieldGetter(name, fast, false);
        }

        public JSObject valueOf()
        {
            return this;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            if (fields == null)
                return JSObject.EmptyEnumerator;
            return fields.Keys.GetEnumerator();
        }
    }
}
