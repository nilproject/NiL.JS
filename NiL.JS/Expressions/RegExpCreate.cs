using System;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class RegExpExpression : Expression
    {
        private string pattern;
        private string flags;

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
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

        public static CodeNode Parse(ParseInfo state, ref int position)
        {
            var i = position;
            if (!Parser.ValidateRegex(state.Code, ref i, false))
                return null;

            string value = state.Code.Substring(position, i - position);
            position = i;

            state.Code = Tools.removeComments(state.SourceCode, i);
            var s = value.LastIndexOf('/') + 1;
            string flags = value.Substring(s);
            try
            {
                return new RegExpExpression(value.Substring(1, s - 2), flags); // объекты должны быть каждый раз разные
            }
            catch (Exception e)
            {
                if (state.message != null)
                    state.message(MessageLevel.Error, i - value.Length, value.Length, string.Format(Strings.InvalidRegExp, value));
                return new ExpressionWrapper(new Throw(e));
            }
        }

        protected internal override CodeNode[] GetChildsImpl()
        {
            return null;
        }

        public override JSValue Evaluate(Context context)
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
