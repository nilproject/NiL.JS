using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class DebuggerOperator : Statement
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "debugger", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            int pos = index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DebuggerOperator()
                {
                    Position = pos,
                    Length = index - pos
                }
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.raiseDebugger(this);
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