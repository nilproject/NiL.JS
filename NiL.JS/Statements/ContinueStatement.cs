using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal class ContinueStatement : Statement
    {
        private JSObject label;
        
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "continue", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            if (state.AllowContinue <= 0)
                throw new ArgumentException();
            while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(code, ref i))
                label = Tools.Unescape(code.Substring(sl, i - sl));
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ContinueStatement()
                {
                    label = label
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Continue;
            context.abortInfo = label;
            return JSObject.undefined;
        }
    }
}