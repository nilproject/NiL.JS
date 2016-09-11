using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
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
            if (val._valueType == JSValueType.Integer
                || val.ValueType == JSValueType.Boolean)
            {
                if (val._iValue == 0)
                {
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = -0.0;
                }
                else
                {
                    if (val._iValue == int.MinValue)
                    {
                        tempContainer._valueType = JSValueType.Double;
                        tempContainer._dValue = val._iValue;
                    }
                    else
                    {
                        tempContainer._valueType = JSValueType.Integer;
                        tempContainer._iValue = -val._iValue;
                    }
                }
            }
            else
            {
                tempContainer._dValue = -Tools.JSObjectToDouble(val);
                tempContainer._valueType = JSValueType.Double;
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