using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class NumberLessOrEqual : Expression
    {
        protected internal override Core.PredictedType ResultType
        {
            get
            {
                return Core.PredictedType.Bool;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public NumberLessOrEqual(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override Core.JSValue Evaluate(Core.Context context)
        {
            int itemp;
            double dtemp;
            var op = first.Evaluate(context);
            if (op.valueType == Core.JSValueType.Int
            || op.valueType == Core.JSValueType.Bool)
            {
                itemp = op.iValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSValueType.Int
                || op.valueType == Core.JSValueType.Bool)
                {
                    return itemp <= op.iValue;
                }
                else if (op.valueType == Core.JSValueType.Double)
                {
                    return itemp <= op.dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSValue() { attributes = JSObjectAttributesInternal.Temporary };
                    tempContainer.valueType = JSValueType.Int;
                    tempContainer.iValue = itemp;
                    return !More.Check(tempContainer, op, true);
                }
            }
            else if (op.valueType == Core.JSValueType.Double)
            {
                dtemp = op.dValue;
                op = second.Evaluate(context);
                if (op.valueType == Core.JSValueType.Int
                || op.valueType == Core.JSValueType.Bool)
                {
                    return dtemp <= op.iValue;
                }
                else if (op.valueType == Core.JSValueType.Double)
                {
                    return dtemp <= op.dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSValue() { attributes = JSObjectAttributesInternal.Temporary };
                    tempContainer.valueType = JSValueType.Double;
                    tempContainer.dValue = dtemp;
                    return !More.Check(tempContainer, op, true);
                }
            }
            else
            {
                if (tempContainer == null)
                    tempContainer = new JSValue() { attributes = JSObjectAttributesInternal.Temporary };
                var temp = tempContainer;
                temp.Assign(op);
                tempContainer = null;
                var res = !More.Check(temp, second.Evaluate(context), true);
                tempContainer = temp;
                return res;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " <= " + second + ")";
        }
    }
}
