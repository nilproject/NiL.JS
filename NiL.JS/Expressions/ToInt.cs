using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class ToInt : Expression
    {
        public ToInt(CodeNode first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context));
                tempContainer.valueType = JSObjectType.Int;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " | " + second + ")";
        }
    }
}