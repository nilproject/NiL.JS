using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class NotEqualOperator : EqualOperator
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public NotEqualOperator(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override JSValue Evaluate(Context context)
        {
            return base.Evaluate(context).iValue == 0;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " != " + second + ")";
        }
    }
}