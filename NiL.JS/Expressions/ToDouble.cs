using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class ToDouble : Expression
    {
        public ToDouble(CodeNode first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.dValue = Tools.JSObjectToDouble(first.Evaluate(context));
            tempContainer.valueType = JSObjectType.Double;
            return tempContainer;
        }

        public override string ToString()
        {
            return "+" + first;
        }
    }
}