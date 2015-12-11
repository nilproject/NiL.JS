
#define TYPE_SAFE

using System;
using System.Collections.Generic;
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

        internal override bool ResultInTempContainer
        {
            get { return true; }
        }

        public Substract(Expression first, Expression second)
            : base(first, second, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            //lock (this)
            {
#if TYPE_SAFE
                double da = 0.0;
                JSValue f = first.Evaluate(context);
                JSValue s = null;
                long l = 0;
                int a;
                if (f.valueType == JSValueType.Integer
                    || f.valueType == JSValueType.Boolean)
                {
                    a = f.iValue;
                    s = second.Evaluate(context);
                    if (s.valueType == JSValueType.Integer
                    || s.valueType == JSValueType.Boolean)
                    {
                        l = (long)a - s.iValue;
                        //if (l > 2147483647L
                        //    || l < -2147483648L)
                        if (l != (int)l)
                        {
                            tempContainer.dValue = l;
                            tempContainer.valueType = JSValueType.Double;
                        }
                        else
                        {
                            tempContainer.iValue = (int)l;
                            tempContainer.valueType = JSValueType.Integer;
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
                tempContainer.valueType = JSValueType.Double;
                return tempContainer;
#else
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) - Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#endif
            }
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            var res = base.Build(ref _this, expressionDepth,  variables, codeContext, message, stats, opts);
            if (!res)
            {
                if (first is Constant && Tools.JSObjectToDouble(first.Evaluate(null)) == 0.0)
                {
                    _this = new Negation(second);
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