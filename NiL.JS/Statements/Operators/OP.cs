using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class OP : Operator
    {
        public OP(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            throw new NotImplementedException();
        }
    }
}