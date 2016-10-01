using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Division : Expression
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

        public Division(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            int itemp;
            var jstemp = first.Evaluate(context);
            if (jstemp._valueType == JSValueType.Integer
                || jstemp._valueType == JSValueType.Boolean)
            {
                itemp = jstemp._iValue;
                jstemp = second.Evaluate(context);
                if ((jstemp._valueType == JSValueType.Boolean
                    || jstemp._valueType == JSValueType.Integer)
                    && jstemp._iValue > 0
                    && itemp > 0
                    && (itemp % jstemp._iValue) == 0)
                {
                    tempContainer._valueType = JSValueType.Integer;
                    tempContainer._iValue = itemp / jstemp._iValue;
                }
                else
                {
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = itemp / Tools.JSObjectToDouble(jstemp);
                }
                return tempContainer;
            }
            tempContainer._dValue = Tools.JSObjectToDouble(jstemp) / Tools.JSObjectToDouble(second.Evaluate(context));
            tempContainer._valueType = JSValueType.Double;
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}