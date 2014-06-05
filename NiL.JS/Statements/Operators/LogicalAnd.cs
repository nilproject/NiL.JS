using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class LogicalAnd : Operator
    {
        public LogicalAnd(Statement first, Statement second)
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