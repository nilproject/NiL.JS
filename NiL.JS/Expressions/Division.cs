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

        public Division(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            int itemp;
            var jstemp = first.Evaluate(context);
            if (jstemp.valueType == JSObjectType.Int
                || jstemp.valueType == JSObjectType.Bool)
            {
                itemp = jstemp.iValue;
                jstemp = second.Evaluate(context);
                if ((jstemp.valueType == JSObjectType.Bool
                    || jstemp.valueType == JSObjectType.Int)
                    && jstemp.iValue > 0
                    && itemp > 0
                    && (itemp % jstemp.iValue) == 0)
                {
                    tempContainer.valueType = JSObjectType.Int;
                    tempContainer.iValue = itemp / jstemp.iValue;
                }
                else
                {
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = itemp / Tools.JSObjectToDouble(jstemp);
                }
                return tempContainer;
            }
            tempContainer.dValue = Tools.JSObjectToDouble(jstemp) / Tools.JSObjectToDouble(second.Evaluate(context));
            tempContainer.valueType = JSObjectType.Double;
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