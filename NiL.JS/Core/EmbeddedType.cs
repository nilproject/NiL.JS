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

        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public override JSObject toString(JSObject args)
        {
            return ToString();
        }

        [Hidden]
        public override string ToString()
        {
            if (oValue != this || valueType < JSObjectType.Object)
                return base.ToString();
            else
                return GetType().ToString();
        }

        internal protected override JSObject GetMember(string name, bool fast, bool own)
        {
            if (prototype == null)
                prototype = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, fast, own);
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributes.ReadOnly) == 0)
                throw new InvalidOperationException("Try to assign to EmbeddedType");
        }

        [Hidden]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
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
