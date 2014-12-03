using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class NumberAddition : Expression
    {
        protected internal override Core.PredictedType ResultType
        {
            get
            {
                return Core.PredictedType.Number;
            }
        }

        public NumberAddition(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override Core.JSObject Evaluate(Core.Context context)
        {
            int itemp;
            long ltemp;
            double dtemp;
            var op = first.Evaluate(context);
            if (op.valueType == Core.JSObjectType.Int
            || op.valueType == Core.JSObjectType.Bool)
            {
                itemp = op.iValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSObjectType.Int
                || op.valueType == Core.JSObjectType.Bool)
                {
                    ltemp = (long)itemp + op.iValue;
                    if ((int)ltemp == ltemp)
                    {
                        tempContainer.valueType = JSObjectType.Int;
                        tempContainer.iValue = (int)ltemp;
                    }
                    else
                    {
                        tempContainer.valueType = JSObjectType.Double;
                        tempContainer.dValue = (double)ltemp;
                    }
                }
                else if (op.valueType == Core.JSObjectType.Double)
                {
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = itemp + op.dValue;
                }
                else
                {
                    tempContainer.valueType = JSObjectType.Int;
                    tempContainer.iValue = itemp;
                    Addition.Impl(tempContainer, tempContainer, op);
                }
            }
            else if (op.valueType == Core.JSObjectType.Double)
            {
                dtemp = op.dValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSObjectType.Int
                || op.valueType == Core.JSObjectType.Bool)
                {
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = dtemp + op.iValue;
                }
                else if (op.valueType == Core.JSObjectType.Double)
                {
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = dtemp + op.dValue;
                }
                else
                {
                    tempContainer.valueType = JSObjectType.Double;
                    tempContainer.dValue = dtemp;
                    Addition.Impl(tempContainer, tempContainer, op);
                }
            }
            else
            {
                Addition.Impl(tempContainer, op.CloneImpl(), second.Evaluate(context));
            }
            return tempContainer;
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
