using System;
using System.Collections.Generic;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Базовый объект для создания пользовательских встроенных типов. 
    /// Предоставляет возможность переопределить обработчики получения поля объекта 
    /// и получения перечислителя полей объекта.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public abstract class CustomType : JSObject
    {
        protected CustomType()
        {
            valueType = JSValueType.Object;
            oValue = this;
            attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        public new JSValue toString(Arguments args)
        {
            return ToString();
        }

        [Hidden]
        public override string ToString()
        {
            if (oValue != this || valueType < JSValueType.Object)
                return base.ToString();
            else
                return GetType().ToString();
        }

        [Hidden]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            if (fields != null)
                foreach (var r in fields)
                    if (r.Value.IsExist && (!pdef || (r.Value.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                        yield return r.Key;
        }
    }
}
