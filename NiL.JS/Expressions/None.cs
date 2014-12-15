using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class None : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return (second ?? first).ResultType;
            }
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

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message)
        {
            if (second == null && (depth > 2 || first is Expression))
            {
                _this = first;
                return true;
            }
            Parser.Build(ref first, depth + 1, vars, strict, message);
            Parser.Build(ref second, depth + 1, vars, strict, message);
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