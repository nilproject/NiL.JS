using NiL.JS.Core;
using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    internal sealed class Mul : Operator
    {
        public Mul(Statement first, Statement second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) * Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.ValueType = JSObjectType.Double;
                return tempResult;
            }
        }

        public override string ToString()
        {
            if (first is ImmidateValueStatement
                && ((first as ImmidateValueStatement).value.ValueType == JSObjectType.Int)
                && ((first as ImmidateValueStatement).value.iValue == -1))
                return "-" + second;
            return "(" + first + " * " + second + ")";
        }
    }
}