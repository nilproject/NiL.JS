using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class EmptyStatement : Expressions.Expression
    {
        private static readonly EmptyStatement _instance = new EmptyStatement();
        public static EmptyStatement Instance { get { return _instance; } }

        protected internal override bool ResultInTempContainer
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

        public EmptyStatement()
            : base(null, null, false)
        {
        }

        public EmptyStatement(int position)
            : base(null, null, false)
        {
            Position = position;
            Length = 0;
        }

        internal override JSValue Evaluate(Context context)
        {
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (depth < 2)
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