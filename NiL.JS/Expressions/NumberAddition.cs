using System;
using System.Collections.Generic;
using NiL.JS.Core;
#if !PORTABLE
using NiL.JS.Core.JIT;
#endif

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class NumberAddition : Expression
    {
        protected internal override Core.PredictedType ResultType
        {
            get
            {
                var pd = _left.ResultType;
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

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public NumberAddition(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Core.Context context)
        {
            int itemp;
            long ltemp;
            double dtemp;
            var op = _left.Evaluate(context);
            if (op._valueType == Core.JSValueType.Integer)
            {
                itemp = op._iValue;
                op = _right.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer)
                {
                    ltemp = (long)itemp + op._iValue;
                    if ((int)ltemp == ltemp)
                    {
                        _tempContainer._valueType = JSValueType.Integer;
                        _tempContainer._iValue = (int)ltemp;
                    }
                    else
                    {
                        _tempContainer._valueType = JSValueType.Double;
                        _tempContainer._dValue = (double)ltemp;
                    }
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    _tempContainer._valueType = JSValueType.Double;
                    _tempContainer._dValue = itemp + op._dValue;
                }
                else
                {
                    _tempContainer._valueType = JSValueType.Integer;
                    _tempContainer._iValue = itemp;
                    Addition.Impl(_tempContainer, _tempContainer, op);
                }
            }
            else if (op._valueType == Core.JSValueType.Double)
            {
                dtemp = op._dValue;
                op = _right.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer)
                {
                    _tempContainer._valueType = JSValueType.Double;
                    _tempContainer._dValue = dtemp + op._iValue;
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    _tempContainer._valueType = JSValueType.Double;
                    _tempContainer._dValue = dtemp + op._dValue;
                }
                else
                {
                    _tempContainer._valueType = JSValueType.Double;
                    _tempContainer._dValue = dtemp;
                    Addition.Impl(_tempContainer, _tempContainer, op);
                }
            }
            else
            {
                Addition.Impl(_tempContainer, op.CloneImpl(false), _right.Evaluate(context));
            }
            return _tempContainer;
        }
#if !PORTABLE && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var ft = _left.TryCompile(false, false, null, dynamicValues);
            var st = _right.TryCompile(false, false, null, dynamicValues);
            if (ft == st) // null == null
                return null;
            if (ft == null && st != null)
            {
                _right = new CompiledNode(_right, st, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft != null && st == null)
            {
                _left = new CompiledNode(_left, ft, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft.Type == st.Type && (ft.Type == typeof(double) || ft.Type == expectedType))
                return System.Linq.Expressions.Expression.Add(ft, st);
            return System.Linq.Expressions.Expression.Add(
                System.Linq.Expressions.Expression.Convert(ft, typeof(double)),
                System.Linq.Expressions.Expression.Convert(st, typeof(double)));
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " + " + _right + ")";
        }
    }
}
