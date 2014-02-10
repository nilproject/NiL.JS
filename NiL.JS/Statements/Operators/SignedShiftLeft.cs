using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class SignedShiftLeft : Operator
    {
        public SignedShiftLeft(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = (int)(left << Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        public override string ToString()
        {
            return "(" + first + " << " + second + ")";
        }
    }
}