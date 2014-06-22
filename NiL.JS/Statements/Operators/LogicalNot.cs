using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class LogicalNot : Operator
    {
        public LogicalNot(Statement first)
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