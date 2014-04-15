using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    public sealed class Division : Operator
    {
        public Division(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) / Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}