using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Ternary : Expression
    {
        private CodeNode[] threads;

        public override bool IsContextIndependent
        {
            get
            {
                return base.IsContextIndependent
                    && (threads[0] is ImmidateValueStatement || (threads[0] is Expression && (threads[0] as Expression).IsContextIndependent))
                    && (threads[1] is ImmidateValueStatement || (threads[1] is Expression && (threads[1] as Expression).IsContextIndependent));
            }
        }

        public Ternary(CodeNode first, CodeNode second)
            : base(first, second, false)
        {
            if (!(second is ImmidateValueStatement)
                || !((second as ImmidateValueStatement).value.oValue is CodeNode[]))
                throw new ArgumentException("Second");
            threads = ((second as ImmidateValueStatement).value.oValue as CodeNode[]);
            if (threads.Length != 2)
                throw new ArgumentException("Second has invalid length");
        }

        internal override JSObject Invoke(Context context)
        {
            if ((bool)first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            base.Optimize(ref _this, depth, fdepth, vars, strict);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}