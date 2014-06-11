using System;
using System.Collections;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    internal sealed class ThisObject : JSObject
    {
        internal static JSObject thisProto;

        private static JSObject createThisProto()
        {
            thisProto = CreateObject();
            thisProto.oValue = thisProto;
            thisProto.attributes |= JSObjectAttributes.ReadOnly | JSObjectAttributes.Immutable | JSObjectAttributes.DoNotEnum | JSObjectAttributes.DoNotDelete;
            return thisProto;
        }

        private Context context;

        public ThisObject(Context context)
            : base(false)
        {
            this.context = context;
            fields = context.fields;
            valueType = JSObjectType.Object;
            oValue = this;
            prototype = thisProto ?? createThisProto();
            assignCallback = (sender) => { throw new JSException(TypeProxy.Proxy(new ReferenceError("Invalid left-hand side in assignment"))); };
        }

        internal protected override JSObject GetMember(string name, bool create, bool own)
        {
            var res = context.GetVarible(name, create);
            if (res.valueType == JSObjectType.NotExist)
                res.valueType = JSObjectType.NotExistInObject;
            return res;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            return context.fields.Keys.GetEnumerator();
        }

        public override string ToString()
        {
            return "[object global]";
        }
    }
}
