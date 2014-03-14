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
    public abstract class EmbeddedType : JSObject
    {
        protected EmbeddedType()
        {
            ValueType = JSObjectType.Object;
            oValue = this;
        }

        public override JSObject toString(JSObject args)
        {
            return ToString();
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
            return DefaultFieldGetter(name, fast, own);
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
