using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    public abstract class EmbeddedType : JSObject
    {
        protected EmbeddedType()
        {
            ValueType = JSObjectType.Object;
            oValue = this;
        }

        public override string ToString()
        {
            if (oValue != this || ValueType < JSObjectType.Object)
                return base.ToString();
            else
                return GetType().ToString();
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, fast, false);
        }

        public override JSObject valueOf()
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
