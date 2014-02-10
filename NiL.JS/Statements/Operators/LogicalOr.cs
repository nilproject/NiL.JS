using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class LogicalOr : Operator
    {
        public LogicalOr(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var left = first.Invoke(context);

            if ((bool)left)
                return left;
            else
                return second.Invoke(context);
        }

        public override string ToString()
        {
            return "(" + first + " || " + second + ")";
        }
    }
}