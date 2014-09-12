using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Neg : Expression
    {
        public Neg(CodeNode first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            lock (this)
            {
                var val = first.Evaluate(context);
                if (val.valueType == JSObjectType.Int
                    || val.ValueType == JSObjectType.Bool)
                {
                    if (val.iValue == 0)
                    {
                        tempContainer.valueType = JSObjectType.Double;
                        tempContainer.dValue = -0.0;
                    }
                    else
                    {
                        tempContainer.valueType = JSObjectType.Int;
                        tempContainer.iValue = -val.iValue;
                    }
                }
                else
                {
                    tempContainer.dValue = -Tools.JSObjectToDouble(val);
                    tempContainer.valueType = JSObjectType.Double;
                }
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "-" + first;
        }
    }
}