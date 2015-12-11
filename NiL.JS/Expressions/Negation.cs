using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Negation : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number; // -int.MinValue == int.MinValue, но должно быть -int.MinValue == -(double)int.MinValue;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Negation(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var val = first.Evaluate(context);
            if (val.valueType == JSValueType.Integer
                || val.ValueType == JSValueType.Boolean)
            {
                if (val.iValue == 0)
                {
                    tempContainer.valueType = JSValueType.Double;
                    tempContainer.dValue = -0.0;
                }
                else
                {
                    if (val.iValue == int.MinValue)
                    {
                        tempContainer.valueType = JSValueType.Double;
                        tempContainer.dValue = val.iValue;
                    }
                    else
                    {
                        tempContainer.valueType = JSValueType.Integer;
                        tempContainer.iValue = -val.iValue;
                    }
                }
            }
            else
            {
                tempContainer.dValue = -Tools.JSObjectToDouble(val);
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
            return "-" + first;
        }
    }
}