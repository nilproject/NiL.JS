using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    public sealed class DebuggerOperator : Statement
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "debugger", ref i) || !Parser.isIdentificatorTerminator(code[i]))
                return new ParseResult();
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new DebuggerOperator()
            };
        }

        internal override JSObject Invoke(Context context)
        {
            context.raiseDebugger();
            return JSObject.undefined;
        }

        public override string ToString()
        {
            return "";
        }
    }
}