using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class DebuggerOperator : Statement
    {
        private Tools.TextCord textCord;

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "debugger", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            int pos = index;
            var tc = Tools.PositionToTextcord(state.Code, pos);
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DebuggerOperator()
                {
                    Position = pos,
                    Length = index - pos,
                    textCord = tc
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.raiseDebugger(Position, textCord);
            return JSObject.undefined;
        }

        public override string ToString()
        {
            return "";
        }

        protected override Statement[] getChildsImpl()
        {
            return null;
        }
    }
}