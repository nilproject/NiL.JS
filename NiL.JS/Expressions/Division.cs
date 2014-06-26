using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Division : Expression
    {
        public Division(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                tempContainer.dValue = Tools.JSObjectToDouble(first.Invoke(context)) / Tools.JSObjectToDouble(second.Invoke(context));
                tempContainer.valueType = JSObjectType.Double;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " / " + second + ")";
        }
    }
}