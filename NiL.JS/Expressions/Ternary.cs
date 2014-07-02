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

        public Ternary(CodeNode first, CodeNode[] threads)
            : base(first, null, false)
        {
            this.threads = threads;
        }

        internal override JSObject Invoke(Context context)
        {
            if ((bool)first.Invoke(context))
                return threads[0].Invoke(context);
            return threads[1].Invoke(context);
        }

        internal override bool Optimize(ref CodeNode _this, int depth, int fdepth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            Parser.Optimize(ref threads[0], depth, fdepth, vars, strict);
            Parser.Optimize(ref threads[1], depth, fdepth, vars, strict);
            base.Optimize(ref _this, depth, fdepth, vars, strict);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}