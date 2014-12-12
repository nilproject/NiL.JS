using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class ToInt : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        public ToInt(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context));
            tempContainer.valueType = JSObjectType.Int;
            return tempContainer;
        }

        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var st = first.TryCompile(false, false, typeof(int), dynamicValues);
            if (st == null)
                return null;
            if (st.Type == typeof(int))
                return st;
            if (st.Type == typeof(bool))
                return System.Linq.Expressions.Expression.Condition(st, System.Linq.Expressions.Expression.Constant(1), System.Linq.Expressions.Expression.Constant(0));
            if (st.Type == typeof(double))
                return System.Linq.Expressions.Expression.Convert(st, typeof(double));
            return System.Linq.Expressions.Expression.Call(new Func<object, int>(Convert.ToInt32).Method, st);
        }

        public override string ToString()
        {
            return "(" + first + " | 0)";
        }
    }
}