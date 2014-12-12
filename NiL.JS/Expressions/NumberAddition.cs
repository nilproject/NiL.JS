using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Expressions
{
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

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
        }
    }
}
