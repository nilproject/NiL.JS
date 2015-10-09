using System;
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

        public override JSValue Invoke(JSValue thisBind, Arguments args)
        {
            JSValue oVal = null;
            if (args != null && args.length > 0)
                oVal = args[0];
            if ((oVal == null) ||
                (((oVal.valueType >= JSValueType.Object && oVal.oValue == null)
                                        || oVal.valueType <= JSValueType.Undefined)))
                return CreateObject();
            else if (oVal.valueType >= JSValueType.Object && oVal.oValue != null)
                return oVal;

            return oVal.ToObject();
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

        public override string ToString(bool headerOnly)
        {
            return "function Object() { [native code] }";
        }
    }
}
