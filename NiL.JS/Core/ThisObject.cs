using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.JS.Core
{
    internal sealed class ThisObject : JSObject
    {
        public ThisObject(Context context)
        {
            ValueType = ObjectValueType.Object;
            fieldGetter = (n, b) =>
            {
                var res = context.GetField(n);
                if (res.ValueType == ObjectValueType.NoExist)
                    res.ValueType = ObjectValueType.NoExistInObject;
                return res;
            };
            assignCallback = () => { throw new InvalidOperationException("Invalid left-hand side in assignment"); };
        }
    }
}
