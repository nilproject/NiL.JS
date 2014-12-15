using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class NotEqual : Equal
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public NotEqual(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override JSObject Evaluate(Context context)
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