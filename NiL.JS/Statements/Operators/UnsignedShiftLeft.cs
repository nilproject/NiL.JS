using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class UnsignedShiftLeft : Operator
    {
        public UnsignedShiftLeft(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var left = Tools.JSObjectToInt(first.Invoke(context));
            tempResult.iValue = (int)((uint)left << Tools.JSObjectToInt(second.Invoke(context)));
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        public override string ToString()
        {
            return "(" + first + " <<< " + second + ")";
        }
    }
}