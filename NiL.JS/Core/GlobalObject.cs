using System;
using System.Collections.Generic;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class GlobalObject : JSObject
    {
        private static JSObject thisProto;

        internal static void refreshGlobalObjectProto()
        {
            thisProto = CreateObject();
            thisProto.oValue = thisProto;
            thisProto.attributes |= JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.Immutable | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.DoNotDelete;
        }

        private Context context;

        internal override JSObject GetDefaultPrototype()
        {
            return thisProto;
        }

        public GlobalObject(Context context)
            : base()
        {
            attributes = JSValueAttributesInternal.SystemObject;
            this.context = context;
            fields = context.fields;
            valueType = JSValueType.Object;
            oValue = this;
        }

        internal protected override JSValue GetMember(JSValue key, bool forWrite, PropertyScope memberScope)
        {
            if (memberScope < PropertyScope.Super && key.valueType != JSValueType.Symbol)
            {
                var nameStr = key.ToString();
                var res = context.GetVariable(nameStr, forWrite);
                return res;
            }
            return base.GetMember(key, forWrite, memberScope);
        }

        protected internal override IEnumerator<KeyValuePair<string, JSValue>> GetEnumerator(bool hideNonEnumerable, EnumerationMode enumerationMode)
        {
            foreach (var i in Context.globalContext.fields)
                if (i.Value.Exists && (!hideNonEnumerable || (i.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return i;
            foreach (var i in context.fields)
                if (i.Value.Exists && (!hideNonEnumerable || (i.Value.attributes & JSValueAttributesInternal.DoNotEnumerate) == 0))
                    yield return i;
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
