using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    /// <summary>
    /// Базовый объект для создания пользовательских встроенных типов. 
    /// Предоставляет возможность переопределить обработчики получения поля объекта 
    /// и получения перечислителя полей объекта.
    /// </summary>
    [Serializable]
    public abstract class EmbeddedType : JSObject
    {
        protected EmbeddedType()
        {
            valueType = JSObjectType.Object;
            oValue = this;
        }

        public override JSObject toString(JSObject args)
        {
            return ToString();
        }

        [Modules.Hidden]
        public override string ToString()
        {
            if (oValue != this || valueType < JSObjectType.Object)
                return base.ToString();
            else
                return GetType().ToString();
        }

        internal override JSObject GetMember(string name, bool fast, bool own)
        {
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, fast, own);
        }

        public override JSObject valueOf()
        {
            return this;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            if (fields != null)
                foreach (var r in fields)
                    if (r.Value.valueType >= JSObjectType.Undefined)
                        yield return r.Key;
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            foreach (var r in prototype)
                yield return r;
        }
    }
}
