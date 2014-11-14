
#define TYPE_SAFE

using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    internal sealed class Mul : Expression
    {
        public Mul(CodeNode first, CodeNode second)
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

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var res = base.Build(ref _this, depth, vars, strict);
            if (!res)
            {
                var exp = first as Constant;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ToDouble(second);
                    return true;
                }
                exp = second as Constant;
                if (exp != null
                    && Tools.JSObjectToDouble(exp.Evaluate(null)) == 1.0)
                {
                    _this = new ToDouble(first);
                    return true;
                }
            }
            return res;
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