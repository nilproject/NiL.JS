using System;
using System.Collections;
using System.Collections.Generic;

namespace NiL.JS.Core
{
    internal sealed class ThisObject : JSObject
    {
        internal static JSObject thisProto;

        private static JSObject createThisProto()
        {
            thisProto = new JSObject(false) { ValueType = JSObjectType.Object, prototype = JSObject.GlobalPrototype };
            thisProto.oValue = thisProto;
            thisProto.attributes |= ObjectAttributes.ReadOnly | ObjectAttributes.Immutable | ObjectAttributes.DontEnum | ObjectAttributes.DontDelete;
            return thisProto;
        }

        private Context context;

        public ThisObject(Context context)
            : base(false)
        {
            this.context = context;
            fields = context.fields;
            ValueType = JSObjectType.Object;
            oValue = this;
            prototype = thisProto ?? createThisProto();
            assignCallback = (sender) => { throw new InvalidOperationException("Invalid left-hand side in assignment"); };
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            var res = context.GetField(name);
            if (res.ValueType == JSObjectType.NotExist)
                res.ValueType = JSObjectType.NotExistInObject;
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
