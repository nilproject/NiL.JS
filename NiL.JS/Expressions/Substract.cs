
#define TYPE_SAFE

using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class Substract : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Substract(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            //lock (this)
            {
#if TYPE_SAFE
                double da = 0.0;
                JSObject f = first.Evaluate(context);
                JSObject s = null;
                long l = 0;
                int a;
                if (f.valueType == JSObjectType.Int
                    || f.valueType == JSObjectType.Bool)
                {
                    a = f.iValue;
                    s = second.Evaluate(context);
                    if (s.valueType == JSObjectType.Int
                    || s.valueType == JSObjectType.Bool)
                    {
                        l = (long)a - s.iValue;
                        //if (l > 2147483647L
                        //    || l < -2147483648L)
                        if (l != (int)l)
                        {
                            tempContainer.dValue = l;
                            tempContainer.valueType = JSObjectType.Double;
                        }
                        else
                        {
                            tempContainer.iValue = (int)l;
                            tempContainer.valueType = JSObjectType.Int;
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
                tempContainer.dValue = da - Tools.JSObjectToDouble(s);
                tempContainer.valueType = JSObjectType.Double;
                return tempContainer;
#else
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) - Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#endif
            }
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            var res = base.Build(ref _this, depth,variables, state, message, statistic, opts);
            if (!res)
            {
                if (first is Constant
                    && Tools.JSObjectToDouble(first.Evaluate(null)) == 0.0)
                {
                    _this = new Neg(second);
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
            return "(" + first + " - " + second + ")";
        }
    }
}