using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class Ternary : Operator
    {
        private Statement[] threads;

        public override bool IsContextIndependent
        {
            get
            {
                return base.IsContextIndependent
                    && (threads[0] is ImmidateValueStatement || (threads[0] is Operator && (threads[0] as Operator).IsContextIndependent))
                    && (threads[1] is ImmidateValueStatement || (threads[1] is Operator && (threads[1] as Operator).IsContextIndependent));
            }
        }

        public Ternary(Statement first, Statement second)
            : base(first, second, false)
        {
            if (!(second is ImmidateValueStatement)
                || !((second as ImmidateValueStatement).value.oValue is Statement[]))
                throw new ArgumentException("Second");
            threads = ((second as ImmidateValueStatement).value.oValue as Statement[]);
            if (threads.Length != 2)
                throw new ArgumentException("Second has invalid length");
        }

        internal override JSObject Invoke(Context context)
        {
            if ((bool)first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        internal override bool Optimize(ref Statement _this, int depth, Dictionary<string, VaribleDescriptor> vars)
        {
            base.Optimize(ref _this, depth, vars);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}