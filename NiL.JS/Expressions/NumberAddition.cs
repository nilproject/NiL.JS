using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class NumberAddition : Expression
    {
        protected internal override Core.PredictedType ResultType
        {
            get
            {
                var pd = first.ResultType;
                switch (pd)
                {
                    case PredictedType.Double:
                        {
                            return PredictedType.Double;
                        }
                    default:
                        {
                            return PredictedType.Number;
                        }
                }
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
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
            if (op.valueType == Core.JSObjectType.Int)
            {
                itemp = op.iValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSObjectType.Int)
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
                if (op.valueType == Core.JSObjectType.Int)
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

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
