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
            thisProto.attributes |= JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.Immutable | JSValueAttributesInternal.DoNotEnum | JSValueAttributesInternal.DoNotDelete;
        }

        private Context context;

        internal override JSObject GetDefaultPrototype()
        {
            return thisProto;
        }

        public GlobalObject(Context context)
            : base(false)
        {
            attributes = JSValueAttributesInternal.SystemObject;
            this.context = context;
            fields = context.fields;
            valueType = JSValueType.Object;
            oValue = this;
        }

        internal protected override JSValue GetMember(JSValue key, bool forWrite, bool own)
        {
            if (key.valueType != JSValueType.Symbol)
            {
                var nameStr = key.ToString();
                var res = context.GetVariable(nameStr, forWrite);
                return res;
            }
            return base.GetMember(key, forWrite, own);
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            foreach (var i in Context.globalContext.fields)
                if (i.Value.IsExists && (!pdef || (i.Value.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                    yield return i.Key;
            foreach (var i in context.fields)
                if (i.Value.IsExists && (!pdef || (i.Value.attributes & JSValueAttributesInternal.DoNotEnum) == 0))
                    yield return i.Key;
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
