using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.Modules;
using NiL.JS.Core.TypeProxing;

namespace NiL.JS.Core
{
    [Serializable]
    internal class ObjectConstructor : ProxyConstructor
    {
        public override JSObject prototype
        {
            get
            {
                return TypeProxy.GetPrototype(proxy.hostedType);
            }
        }

        public ObjectConstructor(TypeProxy proxy)
            : base(proxy)
        {
            _length = 1;
        }

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
            return res;
        }

        protected override JSObject getDefaultPrototype()
        {
            return TypeProxy.GetPrototype(typeof(Function));
        }

        protected internal override System.Collections.Generic.IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            var pe = proxy.GetEnumeratorImpl(hideNonEnum);
            while (pe.MoveNext())
                yield return pe.Current;
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
