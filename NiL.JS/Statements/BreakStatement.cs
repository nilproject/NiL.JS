using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class BreakStatement : Statement
    {
        private JSObject label;

        public JSObject Label { get { return label; } }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "break", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            if (state.AllowBreak <= 0)
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Invalid use break statement")));
            while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(code, ref i, true, state.strict.Peek()))
            {
                label = Tools.Unescape(code.Substring(sl, i - sl));
                if (!state.Labels.Contains(label.oValue as string))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Try to break to undefined label.")));
            }
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new BreakStatement()
                {
                    label = label
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Break;
            context.abortInfo = label;
            return null;
        }

        public override string ToString()
        {
            return "break" + (label != null ? " " + label : "");
        }
    }
}