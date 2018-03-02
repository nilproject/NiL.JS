using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class LogicalConjunction : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public LogicalConjunction(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var left = _left.Evaluate(context);
            if (!(bool)left)
                return left;
            else
                return _right.Evaluate(context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (message != null && expressionDepth <= 1)
                message(MessageLevel.Warning, Position, 0, "Do not use a logical operator as a conditional statement");
            return base.Build(ref _this, expressionDepth,  variables, codeContext | CodeContext.Conditional, message, stats, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " && " + _right + ")";
        }
    }
}