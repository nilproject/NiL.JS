using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class AssignmentOperatorCache : Expression
    {
        private JSValue secondResult;

        public CodeNode Source { get { return first; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return first.ResultType;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal AssignmentOperatorCache(Expression source)
            : base(source, null, false)
        {

        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            var res = first.EvaluateForWrite(context);
            secondResult = Tools.InvokeGetter(res, context.objectSource);
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            var res = secondResult;
            secondResult = null;
            return res;
        }

        public override string ToString()
        {
            return first.ToString();
        }

        public override int Length
        {
            get
            {
                return first.Length;
            }
            internal set
            {
                first.Length = value;
            }
        }

        public override int Position
        {
            get
            {
                return first.Position;
            }
            internal set
            {
                first.Position = value;
            }
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return first.Childs;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
        }

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            // second будем использовать как флаг isVisited
            if (second != null)
                return false;
            second = first;

            _codeContext = codeContext;

            var res = first.Build(ref _this, expressionDepth, scopeVariables, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            if (!res && first is GetVariableExpression)
                (first as GetVariableExpression).forceThrow = true;
            return res;
        }
    }
}
