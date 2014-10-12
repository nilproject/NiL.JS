using System;
using NiL.JS.Core;
using NiL.JS.Core.JIT;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class EmptyStatement : CodeNode
    {
        private static readonly EmptyStatement _instance = new EmptyStatement();
        public static EmptyStatement Instance { get { return _instance; } }

        public EmptyStatement()
        {
        }

        public EmptyStatement(int position)
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
            return JSObject.undefined;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            _this = null;
            return false;
        }

        public override string ToString()
        {
            return "";
        }
    }
}