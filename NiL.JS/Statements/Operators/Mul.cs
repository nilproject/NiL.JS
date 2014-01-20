using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal sealed class Mul : Operator
    {
        public Mul(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            double left = Tools.JSObjectToDouble(first.Invoke(context));
            tempResult.dValue = left * Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }
    }
}