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

        internal override JSObject Invoke(Context context)
        {
            return !(bool)first.Invoke(context);
        }

        public override string ToString()
        {
            return "!" + first;
        }
    }
}