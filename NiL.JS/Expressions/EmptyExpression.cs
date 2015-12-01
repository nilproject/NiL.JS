using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class EmptyExpression : Expression
    {
        private static readonly EmptyExpression _instance = new EmptyExpression();
        public static EmptyExpression Instance { get { return _instance; } }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Undefined;
            }
        }

        public EmptyExpression()
            : base(null, null, false)
        {
        }

        public EmptyExpression(int position)
            : base(null, null, false)
        {
            Position = position;
            Length = 0;
        }

        public override JSValue Evaluate(Context context)
        {
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            if (expressionDepth < 2)
            {
                _this = null;
                Eliminated = true;
            }
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "";
        }
    }
}