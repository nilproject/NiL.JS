using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToNumberOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ToNumberOperator(Expression first)
            : base(first, null, true)
        {

        }

        internal protected override JSValue Evaluate(Context context)
        {
            return Tools.JSObjectToNumber(first.Evaluate(context), tempContainer);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "+" + first;
        }
    }
}