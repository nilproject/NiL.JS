using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LessOrEqual : More
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public LessOrEqual(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return base.Evaluate(context).iValue == 0;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            baseOptimize(owner);
            if (first.ResultType == PredictedType.Number
                && second.ResultType == PredictedType.Number)
            {
                _this = new NumberLessOrEqual(first, second);
                return;
            }
        }

        public override string ToString()
        {
            return "(" + first + " <= " + second + ")";
        }
    }
}