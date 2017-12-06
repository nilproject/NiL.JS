using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Conditional : Expression
    {
        private Expression[] threads;

        protected internal override bool ContextIndependent
        {
            get
            {
                return base.ContextIndependent
                    && (threads[0] == null || threads[0].ContextIndependent)
                    && (threads[1] == null || threads[1].ContextIndependent);
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

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public IList<Expression> Threads { get { return new ReadOnlyCollection<Expression>(threads); } }

        public Conditional(Expression first, Expression[] threads)
            : base(first, null, false)
        {
            this.threads = threads;
        }

        public override JSValue Evaluate(Context context)
        {
            return (bool)_left.Evaluate(context) ? threads[0].Evaluate(context) : threads[1].Evaluate(context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref _left, expressionDepth + 1, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref threads[0], expressionDepth, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref threads[1], expressionDepth, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);

            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && expressionDepth <= 1)
            {
                if (threads[0] == null && threads[1] == null)
                {
                    if (_left.ContextIndependent)
                    {
                        _this = null;
                        return false;
                    }
                    else
                    {
                        _this = new Comma(_left, new Constant(JSValue.undefined));
                    }
                }
                else if (threads[0] == null)
                {
                    _this = new LogicalDisjunction(_left, threads[1]) { Position = Position, Length = Length };
                    return true;
                }
                else if (threads[1] == null)
                {
                    _this = new LogicalConjunction(_left, threads[0]) { Position = Position, Length = Length };
                    return true;
                }
                else if (_left.ContextIndependent)
                {
                    _this = ((bool)_left.Evaluate(null) ? threads[0] : threads[1]);
                    return false;
                }
            }

            base.Build(ref _this, expressionDepth + 1, variables, codeContext, message, stats, opts);
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, InternalCompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
            for (var i = threads.Length; i-- > 0;)
            {
                var cn = threads[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                threads[i] = cn as Expression;
            }
            if (message != null
                && (threads[0] is Variable || threads[0] is Constant)
                && (threads[1] is Variable || threads[1] is Constant)
                && ResultType == PredictedType.Ambiguous)
                message(MessageLevel.Warning, Position, Length, "Type of an expression is ambiguous");
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            threads[0]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            threads[1]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}