using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalNot : Expression
    {
        public LogicalNot(CodeNode first)
            : base(first, null, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return !(bool)first.Evaluate(context);
        }

        public override string ToString()
        {
            return "!" + first;
        }
    }
}