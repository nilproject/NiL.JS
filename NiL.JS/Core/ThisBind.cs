using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    internal sealed class ThisBind : JSObject
    {
        private static JSObject thisProto;

        internal static JSObject refreshThisBindProto()
        {
            thisProto = CreateObject();
            thisProto.oValue = thisProto;
            thisProto.attributes |= JSObjectAttributesInternal.ReadOnly | JSObjectAttributesInternal.Immutable | JSObjectAttributesInternal.DoNotEnum | JSObjectAttributesInternal.DoNotDelete;
            return thisProto;
        }

        private Context context;

        public ThisBind(Context context)
            : base(false)
        {
            attributes = JSObjectAttributesInternal.SystemObject;
            this.context = context;
            fields = context.fields;
            valueType = JSObjectType.Object;
            oValue = this;
            __proto__ = thisProto ?? refreshThisBindProto();
            assignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new ReferenceError("Invalid left-hand side in assignment"))); };
        }

        internal protected override JSObject GetMember(JSObject name, bool create, bool own)
        {
            var nameStr = name.ToString();
            if (nameStr == "__proto__")
            {
                if (__proto__ == null)
                    __proto__ = thisProto;
                return __proto__;
            }
            var res = context.GetVariable(nameStr, create);
            if (res.valueType == JSObjectType.NotExist)
                res.valueType = JSObjectType.NotExistInObject;
            return res;
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            foreach (var i in Context.globalContext.fields)
                if (i.Value.isExist && (!pdef || (i.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return i.Key;
            foreach (var i in context.fields)
                if (i.Value.isExist && (!pdef || (i.Value.attributes & JSObjectAttributesInternal.DoNotEnum) == 0))
                    yield return i.Key;
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
