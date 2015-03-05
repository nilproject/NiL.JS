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

        internal static JSObject refreshGlobalObjectProto()
        {
            thisProto = CreateObject();
            thisProto.oValue = thisProto;
            thisProto.attributes |= JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete;
            return thisProto;
        }

        private Context context;

        protected override JSObject getDefaultPrototype()
        {
            return thisProto;
        }

        public GlobalObject(Context context)
            : base(false)
        {
            attributes = JSObjectAttributesInternal.SystemObject;
            this.context = context;
            fields = context.fields;
            valueType = JSObjectType.Object;
            oValue = this;
        }

        public override void Assign(NiL.JS.Core.JSObject value)
        {
            throw new JSException((new NiL.JS.BaseLibrary.ReferenceError("Invalid left-hand side")));
        }

        internal protected override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            var nameStr = name.ToString();
            var res = context.GetVariable(nameStr, forWrite);
            return res;
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            foreach (var i in Context.globalContext.fields)
                if (i.Value.IsExist && (!pdef || (i.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return i.Key;
            foreach (var i in context.fields)
                if (i.Value.IsExist && (!pdef || (i.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return i.Key;
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
