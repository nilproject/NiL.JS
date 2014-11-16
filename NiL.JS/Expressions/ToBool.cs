using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class ToBool : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public ToBool(Expression first)
            : base(first, null, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return (bool)first.Evaluate(context);
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            if (first.ResultType == PredictedType.Bool)
                _this = first;
        }

        public override string ToString()
        {
            return "!!" + first;
        }
    }
}
