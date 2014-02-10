using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Ternary : Operator
    {
        private Statement[] threads;

        public Ternary(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            if ((bool)first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            threads = ((second as ImmidateValueStatement).Value.oValue as Statement[]);
            return base.Optimize(ref _this, depth, vars);
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}