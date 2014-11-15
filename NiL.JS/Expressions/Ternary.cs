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
        private Expression[] threads;

        public override bool IsContextIndependent
        {
            get
            {
                return base.IsContextIndependent
                    && (threads[0] is Constant || (threads[0] is Expression && threads[0].IsContextIndependent))
                    && (threads[1] is Constant || (threads[1] is Expression && threads[1].IsContextIndependent));
            }
        }

        public IList<CodeNode> Threads { get { return new ReadOnlyCollection<CodeNode>(threads); } }

        public Ternary(Expression first, Expression[] threads)
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

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner)
        {
            base.Optimize(ref _this, owner);
            for (var i = threads.Length; i-- > 0; )
            {
                var cn = threads[i] as CodeNode;
                cn.Optimize(ref cn, owner);
                threads[i] = cn as Expression;
            }
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}