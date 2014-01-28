using System;
using System.Collections;
using System.Collections.Generic;

namespace NiL.JS.Core
{
    internal sealed class ThisObject : JSObject
    {
        private readonly static JSObject thisProto;

        static ThisObject()
        {
            thisProto = BaseTypes.BaseObject.Prototype.Clone() as JSObject;
            thisProto = new JSObject(false) { ValueType = JSObjectType.Object, oValue = new object(), prototype = thisProto };
            thisProto.attributes |= ObjectAttributes.ReadOnly | ObjectAttributes.Immutable | ObjectAttributes.DontEnum | ObjectAttributes.DontDelete;
        }

        private Context context;

        public ThisObject(Context context)
            : base(false)
        {
            this.context = context;
            fields = context.fields;
            ValueType = JSObjectType.Object;
            oValue = this;
            prototype = thisProto;
            assignCallback = () => { throw new InvalidOperationException("Invalid left-hand side in assignment"); };
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
