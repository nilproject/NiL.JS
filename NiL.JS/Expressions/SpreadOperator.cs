using System;
using System.Linq;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class SpreadOperator : Expression
    {

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Unknown;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public SpreadOperator(Expression source)
            : base(source, null, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            return new JSObject
            {
                oValue = first.Evaluate(context).AsIterable().AsEnumerable().ToArray(),
                valueType = JSValueType.SpreadOperatorResult
            };
        }

        internal protected override JSValue EvaluateForWrite(NiL.JS.Core.Context context)
        {
            throw new NotImplementedException();
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return new CodeNode[] { first };
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            CodeNode f = first;
            var res = first.Build(ref f, depth, variables, codeContext, message, statistic, opts);
            first = f as Expression ?? first;
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "..." + first;
        }
    }
}