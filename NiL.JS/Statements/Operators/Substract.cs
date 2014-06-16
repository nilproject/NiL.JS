
#define TYPE_SAFE

using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Substract : Operator
    {
        public Substract(Statement first, Statement second)
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
                        tempResult.iValue = a - s.iValue;
                        tempResult.valueType = JSObjectType.Int;
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
                tempResult.dValue = da - Tools.JSObjectToDouble(s);
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#else
                tempResult.dValue = Tools.JSObjectToDouble(first.Invoke(context)) - Tools.JSObjectToDouble(second.Invoke(context));
                tempResult.valueType = JSObjectType.Double;
                return tempResult;
#endif
            }
        }

        public override string ToString()
        {
            return "(" + first + " - " + second + ")";
        }
    }
}