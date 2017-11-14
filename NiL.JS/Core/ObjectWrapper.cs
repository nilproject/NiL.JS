using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core
{
    /// <summary>
    /// Объект-контейнер для внешних объектов. 
    /// Так же используется для типов наследников JSValue, имеющих valueType меньше Object, 
    /// с целью имитировать valueType == Object.
    /// </summary>
    /// <remarks>
    /// Был создан так как вместе с объектом требуется ещё хранить его аттрибуты, 
    /// которые могли разъехаться при переприсваиваниях
    /// </remarks>
    internal sealed class ObjectWrapper : JSObject
    {
        internal object instance;

        [Hidden]
        public override object Value
        {
            get
            {
                return instance ?? base.Value;
            }
        }

        [Hidden]
        public ObjectWrapper(object instance, JSObject proto)
        {
            this.instance = instance;
            if (instance is Date)
                _valueType = JSValueType.Date;
            else
                _valueType = JSValueType.Object;
            _oValue = this;
            _attributes = JSValueAttributesInternal.SystemObject;
            if (proto != null)
            {
                _attributes |= proto._attributes & JSValueAttributesInternal.Immutable;
                _objectPrototype = proto;
            }
        }

        [Hidden]
        public ObjectWrapper(object instance)
            : this(instance, instance != null ? Context.CurrentGlobalContext.GetPrototype(instance.GetType()) : null)
        {
        }

        protected internal override JSValue GetProperty(JSValue name, bool forWrite, PropertyScope memberScope)
        {
            var t = instance as JSValue;
            if (t != null)
                return t.GetProperty(name, forWrite, memberScope);

            return base.GetProperty(name, forWrite, memberScope);
        }

        protected internal override void SetProperty(JSValue name, JSValue value, PropertyScope memberScope, bool strict)
        {
            var t = instance as JSValue;
            if (t != null)
                t.SetProperty(name, value, memberScope, strict);
            else
                base.SetProperty(name, value, memberScope, strict);
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            var t = instance as JSValue;
            if (t != null)
                return t.DeleteProperty(name);

            return base.DeleteProperty(name);
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumerationMode)
        {
            var t = instance as JSValue;
            if (t != null)
                return t.GetEnumerator(hideNonEnum, enumerationMode);

            return base.GetEnumerator(hideNonEnum, enumerationMode);
        }
    }
}
