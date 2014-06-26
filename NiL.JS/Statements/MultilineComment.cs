using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    /// <summary>
    /// TODO
    /// </summary>
    internal sealed class MultilineComment : CodeNode
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "/*", ref i))
                return new ParseResult();
            while (i + 1 < state.Code.Length && (state.Code[i] != '*' || state.Code[i + 1] != '/')) i++;
            if (i + 1 >= state.Code.Length)
                throw new JSException(TypeProxy.Proxy(new SyntaxError("Non terminated multiline comment")));
            i += 2;
            try
            {
                return new ParseResult()
                {
                    IsParsed = true,
                    Statement = new MultilineComment(state.Code.Substring(index + 2, i - index - 4))
                    {
                        Length = i - index,
                        Position = index
                    }
                };
            }
            finally
            {
                index = i;
            }
        }

        public string Text { get; private set; }

        public MultilineComment(string text)
        {
            Text = text;
        }

        internal override JSObject Invoke(Context context)
        {
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return "/*" + Text + "*/";
        }
    }
}
