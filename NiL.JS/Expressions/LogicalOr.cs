using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class LogicalOr : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public LogicalOr(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal sealed override JSObject Evaluate(Context context)
        {
            var left = first.Evaluate(context);
            if ((bool)left)
                return left;
            else
                return second.Evaluate(context);
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return base.Build(ref _this, depth, variables, state | _BuildState.Conditional, message, statistic, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " || " + second + ")";
        }
    }
}