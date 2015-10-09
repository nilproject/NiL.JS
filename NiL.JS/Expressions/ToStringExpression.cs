using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToStringExpression : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ToStringExpression(Expression first)
            : base(first, null, true)
        {

        }

        internal protected override JSValue Evaluate(Context context)
        {
            var t = first.Evaluate(context);
            if (t.valueType == JSValueType.String)
                return t;
            tempContainer.valueType = JSValueType.String;
            tempContainer.oValue = t.ToPrimitiveValue_Value_String().ToString();
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " + '')";
        }
    }
}