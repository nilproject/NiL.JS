using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class BitwiseExclusiveDisjunctionOperator : Expression
    {
        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        public BitwiseExclusiveDisjunctionOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context)) ^ Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSValueType.Int;
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " ^ " + second + ")";
        }
    }
}