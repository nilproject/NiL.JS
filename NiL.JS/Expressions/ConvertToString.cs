using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ConvertToString : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.String;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ConvertToString(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
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
            return "( \"\" + " + first + ")";
        }
    }
}