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
            tempResult.ValueType = ObjectValueType.Bool;
            tempResult.iValue = second.Invoke(context).GetField(first.Invoke(context).Value.ToString()).ValueType >= ObjectValueType.Undefined ? 1 : 0;
            return tempResult;
        }
    }
}