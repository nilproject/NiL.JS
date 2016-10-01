using System;
using System.Collections.Generic;
using NiL.JS.Core;
#if !(PORTABLE || NETCORE)
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
            var op = first.Evaluate(context);
            if (op._valueType == Core.JSValueType.Integer)
            {
                itemp = op._iValue;
                op = second.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer)
                {
                    ltemp = (long)itemp + op._iValue;
                    if ((int)ltemp == ltemp)
                    {
                        tempContainer._valueType = JSValueType.Integer;
                        tempContainer._iValue = (int)ltemp;
                    }
                    else
                    {
                        tempContainer._valueType = JSValueType.Double;
                        tempContainer._dValue = (double)ltemp;
                    }
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = itemp + op._dValue;
                }
                else
                {
                    tempContainer._valueType = JSValueType.Integer;
                    tempContainer._iValue = itemp;
                    Addition.Impl(tempContainer, tempContainer, op);
                }
            }
            else if (op._valueType == Core.JSValueType.Double)
            {
                dtemp = op._dValue;
                op = second.Evaluate(context);
                if (op._valueType == Core.JSValueType.Integer)
                {
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = dtemp + op._iValue;
                }
                else if (op._valueType == Core.JSValueType.Double)
                {
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = dtemp + op._dValue;
                }
                else
                {
                    tempContainer._valueType = JSValueType.Double;
                    tempContainer._dValue = dtemp;
                    Addition.Impl(tempContainer, tempContainer, op);
                }
            }
            else
            {
                Addition.Impl(tempContainer, op.CloneImpl(false), second.Evaluate(context));
            }
            return tempContainer;
        }
#if !(PORTABLE || NETCORE) && !NET35
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var ft = first.TryCompile(false, false, null, dynamicValues);
            var st = second.TryCompile(false, false, null, dynamicValues);
            if (ft == st) // null == null
                return null;
            if (ft == null && st != null)
            {
                second = new CompiledNode(second, st, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
                return null;
            }
            if (ft != null && st == null)
            {
                first = new CompiledNode(first, ft, JITHelpers._items.GetValue(dynamicValues) as CodeNode[]);
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
            return "(" + first + " + " + second + ")";
        }
    }
}
