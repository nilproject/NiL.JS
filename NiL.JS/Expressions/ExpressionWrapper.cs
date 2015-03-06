using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class ExpressionWrapper : Expression
    {
        private CodeNode node;

        public CodeNode Node { get { return node; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public ExpressionWrapper(CodeNode node)
        {
            this.node = node;
        }

        internal override JSObject Evaluate(Context context)
        {
            return node.Evaluate(context);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistic statistic, Options opts)
        {
            return node.Build(ref node, depth,variables, state, message, statistic, opts);
        }
    }
}
