using System;
using System.Collections;
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
            thisProto.attributes |= JSObjectAttributes.ReadOnly | JSObjectAttributes.Immutable | JSObjectAttributes.DoNotEnum | JSObjectAttributes.DoNotDelete;
            return thisProto;
        }

        private Context context;

        public ThisBind(Context context)
            : base(false)
        {
            attributes = JSObjectAttributes.SystemConstant;
            this.context = context;
            fields = context.fields;
            valueType = JSObjectType.Object;
            oValue = this;
            prototype = thisProto ?? refreshThisBindProto();
            assignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new ReferenceError("Invalid left-hand side in assignment"))); };
        }

        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            var res = context.GetVariable(name, create);
            if (res.valueType == JSObjectType.NotExist)
                res.valueType = JSObjectType.NotExistInObject;
            return res;
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool pdef)
        {
            return context.fields.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
