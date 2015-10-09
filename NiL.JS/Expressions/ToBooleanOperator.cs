using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToBooleanOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public ToBooleanOperator(Expression first)
            : base(first, null, false)
        {

        }

        internal protected override JSValue Evaluate(Context context)
        {
            return (bool)first.Evaluate(context);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "!!" + first;
        }
    }
}
