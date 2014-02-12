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
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Invalid use continue statement")));
            while (char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
            int sl = i;
            JSObject label = null;
            if (Parser.ValidateName(code, ref i, true, state.strict.Peek()))
            {
                label = Tools.Unescape(code.Substring(sl, i - sl));
                if (!state.Labels.Contains(label.oValue as string))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Try to continue to undefined label.")));
            }
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
            return null;
        }

        public override string ToString()
        {
            return "continue" + (label != null ? " " + label : "");
        }
    }
}