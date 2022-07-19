using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class NullishCoalescing : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                var leftType = _left.ResultType;
                var rightType = _right.ResultType;
                return leftType == rightType ? leftType : PredictedType.Ambiguous;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public NullishCoalescing(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var left = _left.Evaluate(context);
            if (left.Defined && !left.IsNull)
                return left;
            else
                return _right.Evaluate(context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return base.Build(ref _this, expressionDepth,  variables, codeContext | CodeContext.Conditional, message, stats, opts);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + " ?? " + _right + ")";
        }
    }
}