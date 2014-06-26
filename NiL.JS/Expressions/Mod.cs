using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Mod : Expression
    {
        public Mod(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                double left = Tools.JSObjectToDouble(first.Invoke(context));
                tempContainer.dValue = left % Tools.JSObjectToDouble(second.Invoke(context));
                tempContainer.valueType = JSObjectType.Double;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " % " + second + ")";
        }
    }
}