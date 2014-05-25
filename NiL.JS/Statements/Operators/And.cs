using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class And : Operator
    {
        public And(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt(first.Invoke(context));
                tempResult.iValue = left & Tools.JSObjectToInt(second.Invoke(context));
                tempResult.ValueType = JSObjectType.Int;
                return tempResult;
            }
        }

        public override string ToString()
        {
            return "(" + first + " & " + second + ")";
        }
    }
}