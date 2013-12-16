using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Assign : Operator
    {
        public Assign(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var val = second.Invoke(context);
            first.Invoke(context).Assign(val);
            return val;
        }
    }
}
