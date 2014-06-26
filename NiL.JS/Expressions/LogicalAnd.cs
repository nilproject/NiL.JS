using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalAnd : Expression
    {
        public LogicalAnd(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var left = first.Invoke(context);
                if (!(bool)left)
                    return left;
                else
                    return second.Invoke(context);
            }
        }

        public override string ToString()
        {
            return "(" + first + " && " + second + ")";
        }
    }
}