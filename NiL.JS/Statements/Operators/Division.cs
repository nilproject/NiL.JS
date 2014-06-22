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
                tempContainer.dValue = Tools.JSObjectToDouble(first.Invoke(context)) / Tools.JSObjectToDouble(second.Invoke(context));
                tempContainer.valueType = JSObjectType.Double;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}