using System;
using System.Collections.Generic;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    /// <summary>
    /// Базовый объект для создания пользовательских встроенных типов. 
    /// Предоставляет возможность переопределить обработчики получения поля объекта 
    /// и получения перечислителя полей объекта.
    /// </summary>
    [Serializable]
    public abstract class CustomType : JSObject
    {
        protected CustomType()
        {
            valueType = JSObjectType.Object;
            oValue = this;
            __proto__ = TypeProxy.GetPrototype(this.GetType());
            attributes |= JSObjectAttributes.SystemObject;
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
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            return DefaultFieldGetter(name, fast, own);
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributes.ReadOnly) == 0)
                throw new InvalidOperationException("Try to assign to " + this.GetType().Name);
        }

        [Hidden]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            if (fields != null)
                foreach (var r in fields)
                    if (r.Value.valueType >= JSObjectType.Undefined)
                        yield return r.Key;
            if (__proto__ == null)
                __proto__ = TypeProxy.GetPrototype(this.GetType());
            foreach (var r in __proto__)
                yield return r;
        }
    }
}
