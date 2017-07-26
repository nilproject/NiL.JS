using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class BitwiseNegation : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public BitwiseNegation(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            _tempContainer._iValue = ~Tools.JSObjectToInt32(_left.Evaluate(context));
            _tempContainer._valueType = JSValueType.Integer;
            return _tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "~" + _left;
        }
    }
}