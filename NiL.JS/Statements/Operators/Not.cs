using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Not : Operator
    {
        public Not(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            tempResult.iValue = Tools.JSObjectToInt(first.Invoke(context)) ^ -1;
            tempResult.ValueType = JSObjectType.Int;
            return tempResult;
        }

        public override string ToString()
        {
            return "~" + first;
        }
    }
}