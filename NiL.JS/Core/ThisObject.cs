using System;

namespace NiL.JS.Core
{
    internal sealed class ThisObject : JSObject
    {
        public ThisObject(Context context)
        {
            ValueType = ObjectValueType.Object;
            oValue = context;
            assignCallback = () => { throw new InvalidOperationException("Invalid left-hand side in assignment"); };
        }

        public override JSObject GetField(string name, bool fast, bool own)
        {
            var res = (oValue as Context).GetField(name);
            if (res.ValueType == ObjectValueType.NotExist)
                res.ValueType = ObjectValueType.NotExistInObject;
            return res;
        }
    }
}
