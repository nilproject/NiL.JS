using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ContinueStatement : CodeNode
    {
        private JSObject label;

        public JSObject Label { get { return label; } }

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
            if (Parser.ValidateName(code, ref i, state.strict.Peek()))
            {
                label = Tools.Unescape(code.Substring(sl, i - sl), state.strict.Peek());
                if (!state.Labels.Contains(label.oValue as string))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("Try to continue to undefined label.")));
            }
            int pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ContinueStatement()
                {
                    label = label,
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Continue;
            context.abortInfo = label;
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            return null;
        }

        public override string ToString()
        {
            return "continue" + (label != null ? " " + label : "");
        }
    }
}