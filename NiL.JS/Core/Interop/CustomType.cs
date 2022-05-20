using System;
using System.Collections.Generic;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Interop
{
    /// <summary>
    /// Базовый объект для создания пользовательских встроенных типов. 
    /// Предоставляет возможность переопределить обработчики получения поля объекта 
    /// и получения перечислителя полей объекта.
    /// </summary>
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public abstract class CustomType : JSObject
    {
        protected CustomType()
        {
            _valueType = JSValueType.Object;
            _oValue = this;
            _attributes |= JSValueAttributesInternal.SystemObject | JSValueAttributesInternal.ReadOnly;
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
            if (_oValue != this || _valueType < JSValueType.Object)
                return base.ToString();
            else
                return GetType().ToString();
        }

        [Hidden]
        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumerationMode, PropertyScope propertyScope = PropertyScope.Common)
        {
            return base.GetEnumerator(hideNonEnum, enumerationMode, propertyScope);
        }
    }
}
