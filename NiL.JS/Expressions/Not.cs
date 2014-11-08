using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Not : Expression
    {
        public Not(CodeNode first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context)) ^ -1;
            tempContainer.valueType = JSObjectType.Int;
            return tempContainer;
        }

        public override string ToString()
        {
            return "~" + first;
        }
    }
}