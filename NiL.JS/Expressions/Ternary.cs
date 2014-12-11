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

        protected internal override PredictedType ResultType
        {
            get
            {
                var ftt = threads[0].ResultType;
                var stt = threads[1].ResultType;
                if (ftt == stt)
                    return ftt;
                if (Tools.IsEqual(ftt, stt, PredictedType.Group))
                    return ftt & PredictedType.Group;
                return PredictedType.Ambiguous;
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
            return (bool)first.Evaluate(context) ? threads[0].Evaluate(context) : threads[1].Evaluate(context);
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message)
        {
            Parser.Build(ref threads[0], depth, vars, strict, message);
            Parser.Build(ref threads[1], depth, vars, strict, message);
            base.Build(ref _this, depth, vars, strict, message);
            if (first is Constant)
            {
                _this = (bool)first.Evaluate(null) ? threads[0] : threads[1];
            }
            else
            {
                if (threads[0] == null
                    && threads[1] == null)
                    _this = first;
                else if (threads[0] == null)
                    _this = new LogicalOr(first, threads[1]) { Position = Position, Length = Length };
                else if (threads[1] == null)
                    _this = new LogicalAnd(first, threads[0]) { Position = Position, Length = Length };
            }
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            base.Optimize(ref _this, owner, message);
            for (var i = threads.Length; i-- > 0; )
            {
                var cn = threads[i] as CodeNode;
                cn.Optimize(ref cn, owner, message);
                threads[i] = cn as Expression;
            }
            if (message != null && ResultType == PredictedType.Ambiguous)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position), "Type of a expression is ambiguous");
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}