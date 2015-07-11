using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class None : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return (second ?? first).ResultType;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public None(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            JSObject temp = null;
            temp = first.Evaluate(context);
            if (second != null)
            {
                if (context != null)
                    context.objectSource = null;
                temp = second.Evaluate(context);
            }
            if (context != null)
                context.objectSource = null;
            return temp;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            codeContext = state;

            if (message != null && depth <= 1 && first != null && second != null)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, 0), "Do not use comma as a statements delimiter");
            if (second == null && (depth > 2 || first is Expression))
            {
                _this = first;
                return true;
            }
            Parser.Build(ref first, depth + 1,variables, state, message, statistic, opts);
            Parser.Build(ref second, depth + 1,variables, state, message, statistic, opts);
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + (second != null ? ", " + second : "") + ")";
        }
    }
}