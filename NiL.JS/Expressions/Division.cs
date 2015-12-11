using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
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
            if (jstemp.valueType == JSValueType.Integer
                || jstemp.valueType == JSValueType.Boolean)
            {
                itemp = jstemp.iValue;
                jstemp = second.Evaluate(context);
                if ((jstemp.valueType == JSValueType.Boolean
                    || jstemp.valueType == JSValueType.Integer)
                    && jstemp.iValue > 0
                    && itemp > 0
                    && (itemp % jstemp.iValue) == 0)
                {
                    tempContainer.valueType = JSValueType.Integer;
                    tempContainer.iValue = itemp / jstemp.iValue;
                }
                else
                {
                    tempContainer.valueType = JSValueType.Double;
                    tempContainer.dValue = itemp / Tools.JSObjectToDouble(jstemp);
                }
                return tempContainer;
            }
            tempContainer.dValue = Tools.JSObjectToDouble(jstemp) / Tools.JSObjectToDouble(second.Evaluate(context));
            tempContainer.valueType = JSValueType.Double;
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