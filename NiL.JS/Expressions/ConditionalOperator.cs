using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ConditionalOperator : Expression
    {
        private Expression[] threads;

        public override bool IsContextIndependent
        {
            get
            {
                return base.IsContextIndependent
                    && (threads[0].IsContextIndependent)
                    && (threads[1].IsContextIndependent);
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

        public ConditionalOperator(Expression first, Expression[] threads)
            : base(first, null, false)
        {
            this.threads = threads;
        }

        public override JSValue Evaluate(Context context)
        {
            return (bool)first.Evaluate(context) ? threads[0].Evaluate(context) : threads[1].Evaluate(context);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref threads[0], depth, variables, state | CodeContext.Conditional | CodeContext.InExpression, message, statistic, opts);
            Parser.Build(ref threads[1], depth, variables, state | CodeContext.Conditional | CodeContext.InExpression, message, statistic, opts);
            base.Build(ref _this, depth, variables, state, message, statistic, opts);
            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && first is ConstantDefinition)
            {
                _this = ((bool)first.Evaluate(null) ? threads[0] : threads[1]);
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Constant expression.");
            }
            else
            {
                if (statistic != null
                    && threads[0] != null
                    && !statistic.ContainsWith
                    && (first is VariableReference && threads[0] is VariableReference)
                    && (first as VariableReference).descriptor == (threads[0] as VariableReference).descriptor)
                {
                    if (threads[0] == null)
                        _this = first;
                    else
                        _this = new LogicalDisjunctionOperator(first, threads[1]) { Position = Position, Length = Length };
                }
                else
                {
                    // Эти оптимизации работают только в тех случаях, когда результат выражения нигде не используется.
                    if (threads[0] == null
                        && threads[1] == null)
                    {
                        _this = first;
                        return true; // можно попытаться удалить и это
                    }
                    else if (threads[0] == null)
                        _this = new LogicalDisjunctionOperator(first, threads[1]) { Position = Position, Length = Length };
                    else if (threads[1] == null)
                        _this = new LogicalConjunctionOperator(first, threads[0]) { Position = Position, Length = Length };
                }
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            base.Optimize(ref _this, owner, message, opts, statistic);
            for (var i = threads.Length; i-- > 0; )
            {
                var cn = threads[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                threads[i] = cn as Expression;
            }
            if (message != null
                && (threads[0] is GetVariableExpression || threads[0] is ConstantDefinition)
                && (threads[1] is GetVariableExpression || threads[1] is ConstantDefinition)
                && ResultType == PredictedType.Ambiguous)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Type of an expression is ambiguous");
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}