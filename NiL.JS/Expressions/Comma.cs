using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class Comma : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return (_right ?? _left).ResultType;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public Comma(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            JSValue temp = null;
            temp = _left.Evaluate(context);
            if (_right != null)
            {
                if (context != null)
                    context._objectSource = null;
                temp = _right.Evaluate(context);
            }
            if (context != null)
                context._objectSource = null;
            if (temp._valueType >= JSValueType.Object)
                return temp._oValue as JSValue ?? temp;
            return temp;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, InternalCompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            this._codeContext = codeContext;

            if (message != null && expressionDepth<= 1 && _left != null && _right != null)
                message(MessageLevel.Warning, Position, 0, "Do not use comma as a statements delimiter");
            if (_right == null)
            {
                _this = _left;
                return true;
            }
            Parser.Build(ref _left, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref _right, expressionDepth + 1,  variables, codeContext | CodeContext.InExpression, message, stats, opts);
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + _left + (_right != null ? ", " + _right : "") + ")";
        }
    }
}