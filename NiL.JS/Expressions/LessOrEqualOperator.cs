using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class LessOrEqualOperator : MoreOperator
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public LessOrEqualOperator(Expression first, Expression second)
            : base(first, second)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            return base.Evaluate(context).iValue == 0;
        }

        internal protected override void Optimize<T>(ref T _this, FunctionNotation owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            baseOptimize(ref _this, owner, message, opts, statistic);
            if (_this == this)
                if (first.ResultType == PredictedType.Number && second.ResultType == PredictedType.Number)
                {
                    _this = new NumberLessOrEqualOperator(first, second) as T;
                    return;
                }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " <= " + second + ")";
        }
    }
}