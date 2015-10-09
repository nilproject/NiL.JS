using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToUnsignedIntegerExpression : Expression
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

        public ToUnsignedIntegerExpression(Expression first)
            : base(first, null, true)
        {

        }

        internal protected override JSValue Evaluate(Context context)
        {
            var t = (uint)Tools.JSObjectToInt32(first.Evaluate(context));
            if (t <= int.MaxValue)
            {
                tempContainer.iValue = (int)t;
                tempContainer.valueType = JSValueType.Int;
            }
            else
            {
                tempContainer.dValue = (double)t;
                tempContainer.valueType = JSValueType.Double;
            }
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " | 0)";
        }
    }
}