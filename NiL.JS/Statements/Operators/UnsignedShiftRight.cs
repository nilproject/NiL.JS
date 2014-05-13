using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class UnsignedShiftRight : Operator
    {
        public UnsignedShiftRight(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.dValue = (double)((uint)left >> Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }

        public override string ToString()
        {
            return "(" + first + " >>> " + second + ")";
        }
    }
}