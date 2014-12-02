using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.TypeProxing
{
    /// <summary>
    /// Создаёт объект-контейнер для внешнего объекта. 
    /// Для типов наследников JSObject, имеющих valueType меньше Object, 
    /// будет так же будет создан и будет имитировать valueType == Object.
    /// Был создан так как вместе с объектом требуется ещё хранить его аттрибуты, 
    /// которые могли разъехаться при переприсваиваниях
    /// </summary>
    internal sealed class ProxyContainer : JSObject
    {
        private object instance;

        [Hidden]
        public override object Value
        {
            get
            {
                return instance;
            }
        }

        [Hidden]
        public ProxyContainer(object instance, JSObject proto)
        {
            __prototype = proto;
            this.instance = instance;
            valueType = JSObjectType.Object;
            oValue = this;
            attributes = JSObjectAttributesInternal.SystemObject;
        }

        [Hidden]
        public ProxyContainer(object instance)
            : this(instance, TypeProxy.GetPrototype(instance.GetType()))
        {
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to " + this.GetType().Name);
            }
        }

        protected internal override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            oValue = instance as JSObject ?? this;
            try
            {
                return base.GetMember(name, forWrite, own);
            }
            finally
            {
                oValue = this;
            }
        }

        internal override void SetMember(JSObject name, JSObject value, bool strict)
        {
            oValue = instance as JSObject ?? this;
            try
            {
                base.SetMember(name, value, strict);
            }
            finally
            {
                oValue = this;
            }
        }

        internal override bool DeleteMember(JSObject name)
        {
            oValue = instance as JSObject ?? this;
            try
            {
                return base.DeleteMember(name);
            }
            finally
            {
                oValue = this;
            }
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            oValue = instance as JSObject ?? this;
            if (oValue == this)
                return base.GetEnumeratorImpl(hideNonEnum);
            try
            {
                return base.GetEnumerator(hideNonEnum);
            }
            finally
            {
                oValue = this;
            }
        }
    }
}
