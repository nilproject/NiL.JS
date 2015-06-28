using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class RegExpExpression : Expression
    {
        private string pattern;
        private string flags;

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

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

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "/" + pattern + "/" + flags;
        }
    }
}
