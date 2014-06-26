
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

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
#if TYPE_SAFE
                double da = 0.0;
                JSObject f = first.Invoke(context);
                JSObject s = null;
                if (f.valueType == JSObjectType.Int
                    || f.valueType == JSObjectType.Bool)
                {
                    int a = f.iValue;
                    s = second.Invoke(context);
                    if (s.valueType == JSObjectType.Int
                    || s.valueType == JSObjectType.Bool)
                    {
                        if (((a | s.iValue) & 0xffff0000) == 0)
                        {
                            tempContainer.iValue = a * s.iValue;
                            tempContainer.valueType = JSObjectType.Int;
                        }
                        else
                        {
                            tempContainer.dValue = a * (double)s.iValue;
                            tempContainer.valueType = JSObjectType.Double;
                        }
                        return tempContainer;
                    }
                    else
                        da = a;
                }
                else
                {
                    da = Tools.JSObjectToDouble(f);
                    s = second.Invoke(context);
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

        public override string ToString()
        {
            if (first is ImmidateValueStatement
                && ((first as ImmidateValueStatement).value.valueType == JSObjectType.Int)
                && ((first as ImmidateValueStatement).value.iValue == -1))
                return "-" + second;
            return "(" + first + " * " + second + ")";
        }
    }
}