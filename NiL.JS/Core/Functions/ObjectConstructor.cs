using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
#if !PORTABLE
    [Serializable]
#endif
    internal class ObjectConstructor : ProxyConstructor
    {
        public ObjectConstructor(TypeProxy proxy)
            : base(proxy)
        {
            _length = new Number(1);
        }

        protected override NiL.JS.Core.JSValue Invoke(bool construct, NiL.JS.Core.JSValue targetObject, NiL.JS.Core.Arguments arguments)
        {
            JSValue oVal = null;
            if (arguments != null && arguments.length > 0)
                oVal = arguments[0];
            if ((oVal == null) 
                || ((oVal.valueType >= JSValueType.Object && oVal.oValue == null) 
                    || oVal.valueType <= JSValueType.Undefined))
                return CreateObject();
            else if (oVal.valueType >= JSValueType.Object && oVal.oValue != null)
                return oVal;

            return oVal.ToObject();
        }

        protected internal override JSValue ConstructObject()
        {
            return null;
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnum, EnumerationMode enumerationMode)
        {
            var pe = proxy.GetEnumerator(hideNonEnum, enumerationMode);
            while (pe.MoveNext())
                yield return pe.Current;
            pe = __proto__.GetEnumerator(hideNonEnum, enumerationMode);
            while (pe.MoveNext())
                yield return pe.Current;
        }

        public override string ToString(bool headerOnly)
        {
            return "function Object() { [native code] }";
        }
    }
}
