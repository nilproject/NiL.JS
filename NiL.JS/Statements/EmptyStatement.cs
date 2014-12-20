using System;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class EmptyStatement : Expressions.Expression
    {
        private static readonly EmptyStatement _instance = new EmptyStatement();
        public static EmptyStatement Instance { get { return _instance; } }

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

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return JITHelpers.UndefinedConstant;
        }

#endif

        internal override JSObject Evaluate(Context context)
        {
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message, FunctionStatistic statistic)
        {
            if (depth < 2)
                _this = null;
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