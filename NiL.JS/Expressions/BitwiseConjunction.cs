using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class BitwiseConjunction : Expression
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

        public BitwiseConjunction(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            _tempContainer._iValue = Tools.JSObjectToInt32(_left.Evaluate(context)) & Tools.JSObjectToInt32(_right.Evaluate(context));
            _tempContainer._valueType = JSValueType.Integer;
            return _tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " & " + _right + ")";
        }
    }
}