using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class SignedShiftRight : Operator
    {
        public SignedShiftRight(Statement first, Statement second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt(first.Invoke(context));
                tempResult.iValue = (int)(left >> Tools.JSObjectToInt(second.Invoke(context)));
                tempResult.valueType = JSObjectType.Int;
                return tempResult;
            }
        }

        public override string ToString()
        {
            return "(" + first + " >> " + second + ")";
        }
    }
}