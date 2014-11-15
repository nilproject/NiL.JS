using System;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    [Serializable]
    internal sealed class RegExpExpression : Expression
    {
        private string pattern;
        private string flags;

        public RegExpExpression(string pattern, string flags)
        {
            this.pattern = pattern;
            this.flags = flags;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        internal override JSObject Evaluate(Context context)
        {
            return new RegExp(pattern, flags);
        }

        public override string ToString()
        {
            return "/" + pattern + "/" + flags;
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            return System.Linq.Expressions.Expression.Call(
                       System.Linq.Expressions.Expression.Constant(this),
                       JITHelpers.methodof(Evaluate),
                       JITHelpers.ContextParameter
                       );
        }

#endif
    }
}
