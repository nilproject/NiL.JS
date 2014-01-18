using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class LogicalNot : Operator
    {
        public LogicalNot(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var val = first.Invoke(context);

            tempResult.iValue = (bool)val ? 0 : 1;
            tempResult.ValueType = JSObjectType.Bool;
            return tempResult;
        }
    }
}