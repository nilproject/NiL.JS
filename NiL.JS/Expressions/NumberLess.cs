using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class NumberLess : Expression
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

        public NumberLess(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Core.Context context)
        {
            int itemp;
            double dtemp;
            var op = first.Evaluate(context);
            if (op._valueType == Core.JSValueType.Integer)
            {
                itemp = op._iValue;
                op = second.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer)
                {
                    return itemp < op._iValue;
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    return itemp < op._dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                    tempContainer._valueType = JSValueType.Integer;
                    tempContainer._iValue = itemp;
                    return Less.Check(tempContainer, op);
                }
            }
            else if (op._valueType == Core.JSValueType.Double)
            {
                dtemp = op._dValue;
                op = second.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer)
                {
                    return dtemp < op._iValue;
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    return dtemp < op._dValue;
                }
                else
                {
                    if (tempContainer == null)
                        tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = dtemp;
                    return Less.Check(tempContainer, op);
                }
            }
            else
            {
                if (tempContainer == null)
                    tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                var temp = tempContainer;
                temp.Assign(op);
                tempContainer = null;
                var res = Less.Check(temp, second.Evaluate(context));
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
            return "(" + first + " < " + second + ")";
        }
    }
}
