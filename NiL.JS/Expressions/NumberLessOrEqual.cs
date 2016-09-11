using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
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

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public NumberLessOrEqual(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Core.Context context)
        {
            int itemp;
            double dtemp;
            var op = first.Evaluate(context);
            if (op._valueType == Core.JSValueType.Integer
            || op._valueType == Core.JSValueType.Boolean)
            {
                itemp = op._iValue;
                op = second.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer
                || op._valueType == Core.JSValueType.Boolean)
                {
                    return itemp <= op._iValue;
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    return itemp <= op._dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                    tempContainer._valueType = JSValueType.Integer;
                    tempContainer._iValue = itemp;
                    return !More.Check(tempContainer, op, true);
                }
            }
            else if (op._valueType == Core.JSValueType.Double)
            {
                dtemp = op._dValue;
                op = second.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer
                || op._valueType == Core.JSValueType.Boolean)
                {
                    return dtemp <= op._iValue;
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    return dtemp <= op._dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = dtemp;
                    return !More.Check(tempContainer, op, true);
                }
            }
            else
            {
                if (tempContainer == null)
                    tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
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
