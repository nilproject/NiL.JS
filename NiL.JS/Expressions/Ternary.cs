using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    && (threads[0] is Constant || (threads[0] is Expression && (threads[0] as Expression).IsContextIndependent))
                    && (threads[1] is Constant || (threads[1] is Expression && (threads[1] as Expression).IsContextIndependent));
            }
        }

        public IList<CodeNode> Threads { get { return new ReadOnlyCollection<CodeNode>(threads); } }

        public Ternary(CodeNode first, CodeNode[] threads)
            : base(first, null, false)
        {
            this.threads = threads;
        }

        internal override JSObject Evaluate(Context context)
        {
            if ((bool)first.Evaluate(context))
                return threads[0].Evaluate(context);
            return threads[1].Evaluate(context);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            Parser.Build(ref threads[0], depth, vars, strict);
            Parser.Build(ref threads[1], depth, vars, strict);
            base.Build(ref _this, depth, vars, strict);
            return false;
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}