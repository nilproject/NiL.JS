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

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            if (first.ResultType == PredictedType.Number)
                _this = first;
        }

        public override string ToString()
        {
            return "+" + first;
        }
    }
}