using System;
using System.Collections.Generic;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core.TypeProxing
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
            attributes |= JSObjectAttributesInternal.SystemObject | JSObjectAttributesInternal.ReadOnly;
        }

        [CLSCompliant(false)]
        [AllowUnsafeCall(typeof(JSObject))]
        [DoNotEnumerate]
        public new JSObject toString(Arguments args)
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

        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            return DefaultFieldGetter(name, forWrite, own);
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

        [Hidden]
        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            if (fields != null)
                foreach (var r in fields)
                    if (r.Value.IsExist && (!pdef || (r.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                        yield return r.Key;
            var penum = __proto__.GetEnumeratorImpl(pdef);
            while (penum.MoveNext())
                yield return penum.Current;
        }
    }
}
