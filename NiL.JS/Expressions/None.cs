using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class None : Expression
    {
        public None(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            JSObject temp = null;
            temp = first.Invoke(context);
            if (second != null)
            {
                if (context != null)
                    context.objectSource = null;
                temp = second.Invoke(context);
            }
            if (context != null)
                context.objectSource = null;
            return temp;
        }

        public override string ToString()
        {
            return "(" + first + (second != null ? ", " + second : "") + ")";
        }
    }
}