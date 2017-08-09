using System;
using System.Collections.Generic;
using System.Reflection;
using NiL.JS.Core;

#if NET40
using NiL.JS.Backward;
#endif

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class ConvertToInteger : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Int;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public ConvertToInteger(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var t = _left.Evaluate(context);
            if (t._valueType == JSValueType.Integer)
                _tempContainer._iValue = t._iValue;
            else
                _tempContainer._iValue = Tools.JSObjectToInt32(t, 0, false);
            _tempContainer._valueType = JSValueType.Integer;
            return _tempContainer;
        }
#if !PORTABLE
        internal override System.Linq.Expressions.Expression TryCompile(bool selfCompile, bool forAssign, Type expectedType, List<CodeNode> dynamicValues)
        {
            var st = _left.TryCompile(false, false, typeof(int), dynamicValues);
            if (st == null)
                return null;
            if (st.Type == typeof(int))
                return st;
            if (st.Type == typeof(bool))
                return System.Linq.Expressions.Expression.Condition(st, System.Linq.Expressions.Expression.Constant(1), System.Linq.Expressions.Expression.Constant(0));
            if (st.Type == typeof(double))
                return System.Linq.Expressions.Expression.Convert(st, typeof(double));
            return System.Linq.Expressions.Expression.Call(new Func<object, int>(Convert.ToInt32).GetMethodInfo(), st);
        }
#endif
        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " | 0)";
        }
    }
}