using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalOr : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public LogicalOr(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var left = first.Evaluate(context);
            if ((bool)left)
                return left;
            else
                return second.Evaluate(context);
        }

        public override string ToString()
        {
            return "(" + first + " || " + second + ")";
        }
    }
}