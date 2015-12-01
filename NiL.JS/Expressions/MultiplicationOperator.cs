
#define TYPE_SAFE

using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class MultiplicationOperator : Expression
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

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public MultiplicationOperator(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
#if TYPE_SAFE
            double da = 0.0;
            JSValue f = first.Evaluate(context);
            JSValue s = null;
            long l = 0;
            if (((int)f.valueType & 0xf) > 3) // bool - b0111, int - b1011
            {
                int a = f.iValue;
                s = second.Evaluate(context);
                if (((int)s.valueType & 0xf) > 3)
                {
                    if (((a | s.iValue) & 0xFFFF0000) == 0)
                    {
                        tempContainer.iValue = a * s.iValue;
                        tempContainer.valueType = JSValueType.Int;
                    }
                    else
                    {
                        l = (long)a * s.iValue;
                        if (l > 2147483647L || l < -2147483648L)
                        {
                            tempContainer.dValue = l;
                            tempContainer.valueType = JSValueType.Double;
                        }
                        else
                        {
                            tempContainer.iValue = (int)l;
                            tempContainer.valueType = JSValueType.Int;
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
            tempContainer.valueType = JSValueType.Double;
            return tempContainer;
#else
            tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) * Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.valueType = JSObjectType.Double;
            return tempResult;
#endif
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res)
            {
                var exp = first as ConstantDefinition;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ToNumberOperator(second);
                    return true;
                }
                exp = second as ConstantDefinition;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ToNumberOperator(first);
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
            if (first is ConstantDefinition
                && ((first as ConstantDefinition).value.valueType == JSValueType.Int)
                && ((first as ConstantDefinition).value.iValue == -1))
                return "-" + second;
            return "(" + first + " * " + second + ")";
        }
    }
}