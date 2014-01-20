using System;

namespace NiL.JS.Core
{
    internal sealed class ThisObject : JSObject
    {
        public ThisObject(Context context)
        {
            ValueType = JSObjectType.Object;
            oValue = context;
            assignCallback = () => { throw new InvalidOperationException("Invalid left-hand side in assignment"); };
            attributes |= ObjectAttributes.DontDelete;
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            var res = (oValue as Context).GetField(name);
            if (res.ValueType == JSObjectType.NotExist)
                res.ValueType = JSObjectType.NotExistInObject;
            return res;
        }
    }
}
