using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class BreakStatement : CodeNode
    {
        private JSValue label;

        public JSValue Label { get { return label; } }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "break", ref i) || !Parser.IsIdentificatorTerminator(state.Code[i]))
                return null;
            while (char.IsWhiteSpace(state.Code[i]) && !Tools.isLineTerminator(state.Code[i]))
                i++;
            int sl = i;
            JSValue label = null;
            if (Parser.ValidateName(state.Code, ref i, state.strict))
            {
                label = Tools.Unescape(state.Code.Substring(sl, i - sl), state.strict);
                if (!state.Labels.Contains(label.oValue.ToString()))
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("Try to break to undefined label.")));
            }
            else if (!state.AllowBreak.Peek())
                ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("Invalid use break statement")));
            var pos = index;
            index = i;
            state.breaksCount++;
            return new BreakStatement()
                {
                    label = label,
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            context.abortType = AbortType.Break;
            context.abortInfo = label;
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
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