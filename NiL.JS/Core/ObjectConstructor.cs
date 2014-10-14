using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;

namespace NiL.JS.Core
{
    [Serializable]
    [Prototype(typeof(Function))]
    internal class ObjectConstructor : Function
    {
        private TypeProxy proxy;

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public override JSObject prototype
        {
            [Hidden]
            get
            {
                return TypeProxy.GetPrototype(proxy.hostedType);
            }
        }

        [Hidden]
        public ObjectConstructor(TypeProxy proxy)
        {
            _length = 1;
            this.proxy = proxy;
        }

        [Hidden]
        public override NiL.JS.Core.JSObject Invoke(JSObject thisBind, Arguments args)
        {
            object oVal = null;
            if (args != null && args.length > 0)
                oVal = args[0];
            JSObject res = null;
            if ((oVal == null) ||
                (oVal is JSObject && (((oVal as JSObject).valueType >= JSObjectType.Object && (oVal as JSObject).oValue == null)
                                        || (oVal as JSObject).valueType <= JSObjectType.Undefined)))
                return CreateObject();
            else if ((oVal as JSObject).valueType >= JSObjectType.Object && (oVal as JSObject).oValue != null)
                return oVal as JSObject;

            res = CreateObject();

            res.valueType = JSObjectType.Object;
            res.oValue = (oVal is JSObject && ((oVal as JSObject).attributes & JSObjectAttributesInternal.SystemObject) != 0) ? (oVal as JSObject).Clone() : oVal;
            if (oVal is JSObject)
                res.__proto__ = (oVal as JSObject).GetMember("__proto__", false, true);
            return res;
        }

        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            if (__proto__ == null)
            {
                __proto__ = TypeProxy.GetPrototype(typeof(ObjectConstructor));
                __proto__.fields.Clear();
            }

            var res = __proto__.GetMember(name, false, own);
            if (res.isExist)
                return res;
            return proxy.GetMember(name, create, own);
        }

        protected internal override System.Collections.Generic.IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            var pe = proxy.GetEnumeratorImpl(hideNonEnum);
            while (pe.MoveNext())
                yield return pe.Current;
            if (__proto__ == null)
            {
                __proto__ = TypeProxy.GetPrototype(typeof(ObjectConstructor));
                proxy.__proto__ = __proto__;
            }
            pe = __proto__.GetEnumeratorImpl(hideNonEnum);
            while (pe.MoveNext())
                yield return pe.Current;
        }

        [Hidden]
        public override string ToString()
        {
            return "function Object() { [native code] }";
        }
    }
}
