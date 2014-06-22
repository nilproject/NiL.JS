
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
                        tempContainer.iValue = a - s.iValue;
                        tempContainer.valueType = JSObjectType.Int;
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

        public override string ToString()
        {
            return "(" + first + " - " + second + ")";
        }
    }
}