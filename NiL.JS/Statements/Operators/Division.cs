using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Division : Operator
    {
        public Division(Statement first, Statement second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) / Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
            }
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}