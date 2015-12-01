using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class CommaOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return (second ?? first).ResultType;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public CommaOperator(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            JSValue temp = null;
            temp = first.Evaluate(context);
            if (second != null)
            {
                if (context != null)
                    context.objectSource = null;
                temp = second.Evaluate(context);
            }
            if (context != null)
                context.objectSource = null;
            if (temp.valueType >= JSValueType.Object)
                return temp.oValue as JSValue ?? temp;
            return temp;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            this._codeContext = codeContext;

            if (message != null && expressionDepth<= 1 && first != null && second != null)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, 0), "Do not use comma as a statements delimiter");
            if (second == null)
            {
                _this = first;
                return true;
            }
            Parser.Build(ref first, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref second, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
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