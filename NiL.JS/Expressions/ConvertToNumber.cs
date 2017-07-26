using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ConvertToNumber : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ConvertToNumber(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            return Tools.JSObjectToNumber(_left.Evaluate(context), _tempContainer);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "+" + _left;
        }
    }
}