using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Division : Expression
    {
        public Division(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.dValue = Tools.JSObjectToDouble(first.Evaluate(context)) / Tools.JSObjectToDouble(second.Evaluate(context));
            tempContainer.valueType = JSObjectType.Double;
            return tempContainer;
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}