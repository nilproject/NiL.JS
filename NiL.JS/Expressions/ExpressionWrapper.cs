using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class ExpressionWrapper : Expression
    {
        private CodeNode node;

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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            return node.Build(ref node, depth, vars, strict);
        }
    }
}
