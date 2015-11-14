using System;
using System.Collections.Generic;
using System.Threading;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class YieldOperator : Expression
    {
        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return true;
            }
        }

        public YieldOperator(Expression first)
            : base(first, null, true)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            if (context.abortType == AbortType.None)
            {
                context.abortInfo = first.Evaluate(context);
                context.abortType = AbortType.Suspend;
                return null;
            }
            else if (context.abortType == AbortType.Resume)
            {
                context.abortType = AbortType.None;
                var result = context.abortInfo;
                context.abortInfo = null;
                return result;
            }
            else
                throw new InvalidOperationException();
        }

        protected internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            statistic.ContainsYield = true;
            return base.Build(ref _this, depth, variables, codeContext, message, statistic, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            first.Decompose(ref first, result);

            if ((_codeContext & CodeContext.InExpression) != 0)
            {
                result.Add(new StoreValueStatement(this));
                self = new ExtractStoredValueExpression(this);
            }
        }

        public override string ToString()
        {
            return "yield " + first;
        }
    }
}