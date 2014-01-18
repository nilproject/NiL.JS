using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class In : Operator
    {
        public In(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var t = second.Invoke(context).GetField(first.Invoke(context).ToString(), true, false);
            tempResult.iValue = t != JSObject.undefined && t.ValueType >= JSObjectType.Undefined ? 1 : 0;
            tempResult.ValueType = JSObjectType.Bool;
            return tempResult;
        }
    }
}