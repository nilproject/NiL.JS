using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class ToNumber : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        public ToNumber(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return Tools.JSObjectToNumber(first.Evaluate(context), tempContainer);
        }

        public override string ToString()
        {
            return "+" + first;
        }
    }
}