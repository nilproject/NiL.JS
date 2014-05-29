using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Ternary : Operator
    {
        private Statement[] threads;

        public Ternary(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            if ((bool)first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, Statement> vars)
        {
            threads = ((second as ImmidateValueStatement).value.oValue as Statement[]);
            Parser.Optimize(ref threads[0], depth + 1, vars);
            Parser.Optimize(ref threads[1], depth + 1, vars);
            return base.Optimize(ref _this, depth, vars);
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}