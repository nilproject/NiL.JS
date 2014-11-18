using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    if (((itemp | op.iValue) & 0x7c000000) == 0
                    && (itemp & op.iValue & int.MinValue/*0x80000000*/) == 0)
                    {
                        tempContainer.valueType = JSObjectType.Int;
                        tempContainer.iValue = itemp + op.iValue;
                    }
                    else
                    {
                        tempContainer.valueType = JSObjectType.Double;
                        tempContainer.dValue = (long)itemp + op.iValue;
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
                tempContainer.Assign(op);
                Addition.Impl(tempContainer, tempContainer, second.Evaluate(context));
            }
            return tempContainer;
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
