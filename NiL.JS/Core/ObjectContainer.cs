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

        protected internal override JSValue GetMember(JSValue name, bool forWrite, MemberScope memberScope)
        {
            var t = instance as JSValue;
            if (t != null)
                return t.GetMember(name, forWrite, memberScope);
            return base.GetMember(name, forWrite, memberScope);
        }

        protected internal override void SetMember(JSValue name, JSValue value, MemberScope memberScope, bool strict)
        {
            var t = instance as JSValue;
            if (t != null)
                t.SetMember(name, value, strict);
            base.SetMember(name, value, strict);
        }

        protected internal override bool DeleteMember(JSValue name)
        {
            var t = instance as JSValue;
            if (t != null)
                return t.DeleteMember(name);
            return base.DeleteMember(name);
        }

        public override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumerationMode)
        {
            var t = instance as JSValue;
            if (t != null)
                return t.GetEnumerator(hideNonEnum, enumerationMode);
            return base.GetEnumerator(hideNonEnum, enumerationMode);
        }
    }
}
