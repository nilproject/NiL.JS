using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToInt : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ToInt(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSValue Evaluate(Context context)
        {
            var t = first.Evaluate(context);
            if (t.valueType == JSValueType.Int)
                tempContainer.iValue = t.iValue;
            else
                tempContainer.iValue = Tools.JSObjectToInt32(t, 0, false);
            tempContainer.valueType = JSValueType.Int;
            return tempContainer;
        }
#if !PORTABLE
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
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " | 0)";
        }
    }
}