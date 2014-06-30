using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    internal class ObjectConstructor : Function
    {
        private TypeProxy proxy;

        public override JSObject length
        {
            get
            {
                if (_length == null)
                    _length = 1;
                return _length;
            }
        }

        public ObjectConstructor(TypeProxy proxy)
        {
            this.proxy = proxy;
        }

        public override NiL.JS.Core.JSObject Invoke(JSObject thisBind, NiL.JS.Core.JSObject args)
        {
            object oVal = null;
            if (args != null && args.GetMember("length").iValue > 0)
                oVal = args.GetMember("0");
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
                __proto__ = TypeProxy.GetPrototype(typeof(Function));
                proxy.__proto__ = __proto__;
            }
            return proxy.GetMember(name, create, own);
        }

        protected internal override System.Collections.Generic.IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            var pe = proxy.GetEnumeratorImpl(hideNonEnum);
            while (pe.MoveNext())
                yield return pe.Current;
            if (__proto__ == null)
            {
                __proto__ = TypeProxy.GetPrototype(typeof(Function));
                proxy.__proto__ = __proto__;
            }
            pe = __proto__.GetEnumeratorImpl(hideNonEnum);
            while (pe.MoveNext())
                yield return pe.Current;
        }

        public override string ToString()
        {
            return "function Object() { [native code] }";
        }
    }
}
