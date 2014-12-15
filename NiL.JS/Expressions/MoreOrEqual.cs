using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class MoreOrEqual : Less
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public MoreOrEqual(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return base.Evaluate(context).iValue == 0;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            baseOptimize(owner, message);
            if (first.ResultType == PredictedType.Number
                && second.ResultType == PredictedType.Number)
            {
                _this = new NumberMoreOrEqual(first, second);
                return;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " >= " + second + ")";
        }
    }
}