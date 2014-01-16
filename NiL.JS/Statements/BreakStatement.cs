using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    class BreakStatement : Statement
    {
        private JSObject label;

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "break", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            if (state.AllowBreak <= 0)
                throw new ArgumentException("break not allowed in this context");
            while (char.IsWhiteSpace(code[i]) && !Parser.isLineTerminator(code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(code, ref i))
                label = code.Substring(sl, i - sl);
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

        public override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Break;
            context.abortInfo = label;
            return JSObject.undefined;
        }

        public override JSObject Invoke(Context context, JSObject args)
        {
            throw new NotImplementedException();
        }
    }
}