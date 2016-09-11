using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;

namespace NiL.JS.Core.Functions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    internal class ObjectConstructor : ProxyConstructor
    {
        public ObjectConstructor(TypeProxy proxy)
            : base(proxy)
        {
            _length = new Number(1);
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            JSValue oVal = null;
            if (arguments != null && arguments.length > 0)
                oVal = arguments[0];
            if ((oVal == null) 
                || ((oVal._valueType >= JSValueType.Object && oVal._oValue == null) 
                    || oVal._valueType <= JSValueType.Undefined))
                return CreateObject();
            else if (oVal._valueType >= JSValueType.Object && oVal._oValue != null)
                return oVal;

            return oVal.ToObject();
        }

        protected internal override JSValue ConstructObject()
        {
            return JSObject.CreateObject();
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
            var result = "function " + name + "()";

            if (!headerOnly)
            {
                result += " { [native code] }";
            }

            return result;
        }
    }
}
