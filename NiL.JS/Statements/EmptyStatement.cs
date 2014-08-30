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

        internal override System.Linq.Expressions.Expression BuildTree(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return JITHelpers.UndefinedConstant;
        }

        internal override JSObject Invoke(Context context)
        {
            return JSObject.undefined;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return "";
        }
    }
}