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
            var op = _left.Evaluate(context);
            if (op._valueType == Core.JSValueType.Integer
            || op._valueType == Core.JSValueType.Boolean)
            {
                itemp = op._iValue;
                op = _right.Evaluate(context);
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
                    if (_tempContainer == null)
                        _tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                    _tempContainer._valueType = JSValueType.Integer;
                    _tempContainer._iValue = itemp;
                    return !More.Check(_tempContainer, op, true);
                }
            }
            else if (op._valueType == Core.JSValueType.Double)
            {
                dtemp = op._dValue;
                op = _right.Evaluate(context);
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
                    if (_tempContainer == null)
                        _tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                    _tempContainer._valueType = JSValueType.Double;
                    _tempContainer._dValue = dtemp;
                    return !More.Check(_tempContainer, op, true);
                }
            }
            else
            {
                if (_tempContainer == null)
                    _tempContainer = new JSValue() { _attributes = JSValueAttributesInternal.Temporary };
                var temp = _tempContainer;
                temp.Assign(op);
                _tempContainer = null;
                var res = !More.Check(temp, _right.Evaluate(context), true);
                _tempContainer = temp;
                return res;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " <= " + _right + ")";
        }
    }
}
