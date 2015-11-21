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
    internal sealed class ObjectContainer : JSObject
    {
        internal bool ownedFieldsOnly;
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
        public ObjectContainer(object instance, JSObject proto)
        {
            this.instance = instance;
            if (instance is Date)
                valueType = JSValueType.Date;
            else
                valueType = JSValueType.Object;
            oValue = this;
            attributes = JSValueAttributesInternal.SystemObject;
            if (proto != null)
            {
                attributes |= proto.attributes & JSValueAttributesInternal.Immutable;
                __prototype = proto;
            }
        }

        [Hidden]
        public ObjectContainer(object instance)
            : this(instance, instance != null ? TypeProxy.GetPrototype(instance.GetType()) : null)
        {
        }

        protected internal override JSValue GetProperty(JSValue name, bool forWrite, PropertyScope memberScope)
        {
            if (!ownedFieldsOnly)
            {
                var t = instance as JSValue;
                if (t != null)
                    return t.GetProperty(name, forWrite, memberScope);
            }
            return base.GetProperty(name, forWrite, memberScope);
        }

        protected internal override void SetProperty(JSValue name, JSValue value, PropertyScope memberScope, bool strict)
        {
            if (!ownedFieldsOnly)
            {
                var t = instance as JSValue;
                if (t != null)
                    t.SetProperty(name, value, memberScope, strict);
            }
            base.SetProperty(name, value, memberScope, strict);
        }

        protected internal override bool DeleteProperty(JSValue name)
        {
            if (!ownedFieldsOnly)
            {
                var t = instance as JSValue;
                if (t != null)
                    return t.DeleteProperty(name);
            }
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
