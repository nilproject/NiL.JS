
#define TYPE_SAFE

using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Mul : Expression
    {
        protected internal override PredictedType ResultType
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

        public Mul(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
#if TYPE_SAFE
                double da = 0.0;
                JSObject f = first.Evaluate(context);
                JSObject s = null;
                long l = 0;
                if (f.valueType == JSObjectType.Int
                    || f.valueType == JSObjectType.Bool)
                {
                    int a = f.iValue;
                    s = second.Evaluate(context);
                    if (s.valueType == JSObjectType.Int
                        || s.valueType == JSObjectType.Bool)
                    {
                        if (((a | s.iValue) & 0xFFFF0000) == 0)
                        {
                            tempContainer.iValue = a * s.iValue;
                            tempContainer.valueType = JSObjectType.Int;
                        }
                        else
                        {
                            l = (long)a * s.iValue;
                            if (l > 2147483647L
                                || l < -2147483648L)
                            {
                                tempContainer.dValue = l;
                                tempContainer.valueType = JSObjectType.Double;
                            }
                            else
                            {
                                tempContainer.iValue = (int)l;
                                tempContainer.valueType = JSObjectType.Int;
                            }
                        }
                        return tempContainer;
                    }
                    else
                        da = a;
                }
                else
                {
                    da = Tools.JSObjectToDouble(f);
                    s = second.Evaluate(context);
                }
                tempContainer.dValue = da * Tools.JSObjectToDouble(s);
                tempContainer.valueType = JSObjectType.Double;
                return tempContainer;
#else
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) * Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#endif
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            var res = base.Build(ref _this, depth,variables, state, message, statistic, opts);
            if (!res)
            {
                var exp = first as Constant;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ToNumber(second);
                    return true;
                }
                exp = second as Constant;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ToNumber(first);
                    return true;
                }
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            if (first is Constant
                && ((first as Constant).value.valueType == JSObjectType.Int)
                && ((first as Constant).value.iValue == -1))
                return "-" + second;
            return "(" + first + " * " + second + ")";
        }
    }
}