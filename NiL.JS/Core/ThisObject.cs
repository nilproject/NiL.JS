using System;

namespace NiL.JS.Core
{
    internal sealed class ThisObject : JSObject
    {
        private Context context;

        public ThisObject(Context context)
        {
            ValueType = JSObjectType.Object;
            this.context = context;
            oValue = this;
            assignCallback = () => { throw new InvalidOperationException("Invalid left-hand side in assignment"); };
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            var res = context.GetField(name);
            if (res.ValueType == JSObjectType.NotExist)
                res.ValueType = JSObjectType.NotExistInObject;
            return res;
        }
    }
}
