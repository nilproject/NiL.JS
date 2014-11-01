using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalOr : Expression
    {
        public LogicalOr(CodeNode first, CodeNode second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var left = first.Evaluate(context);
            if ((bool)left)
                return left;
            else
                return second.Evaluate(context);
        }

        public override string ToString()
        {
            return "(" + first + " || " + second + ")";
        }
    }
}