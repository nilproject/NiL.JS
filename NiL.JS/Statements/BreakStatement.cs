using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class BreakStatement : CodeNode
    {
        private JSObject label;

        public JSObject Label { get { return label; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "break", ref i) || !Parser.isIdentificatorTerminator(state.Code[i]))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
            {
                label = Tools.Unescape(state.Code.Substring(sl, i - sl), state.strict.Peek());
                if (!state.Labels.Contains(label.oValue.ToString()))
                    throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("Try to break to undefined label.")));
            }
            else if (!state.AllowBreak.Peek())
                throw new JSException((new NiL.JS.BaseLibrary.SyntaxError("Invalid use break statement")));
            var pos = index;
            index = i;
            state.breaksCount++;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new BreakStatement()
                {
                    label = label,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Evaluate(Context context)
        {
            context.abort = AbortType.Break;
            context.abortInfo = label;
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "break" + (label != null ? " " + label : "");
        }
    }
}