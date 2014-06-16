
#define TYPE_SAFE

using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    internal sealed class Mul : Operator
    {
        public Mul(Statement first, Statement second)
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
                            tempResult.iValue = a * s.iValue;
                            tempResult.valueType = JSObjectType.Int;
                        }
                        else
                        {
                            tempResult.dValue = a * (double)s.iValue;
                            tempResult.valueType = JSObjectType.Double;
                        }
                        return tempResult;
                    }
                    else
                        da = a;
                }
                else
                {
                    da = Tools.JSObjectToDouble(f);
                    s = second.Invoke(context);
                }
                tempResult.dValue = da * Tools.JSObjectToDouble(s);
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
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