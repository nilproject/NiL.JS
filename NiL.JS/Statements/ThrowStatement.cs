using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ThrowStatement : Statement, IOptimizable
    {
        private Statement body;

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "throw", ref i) || (!char.IsWhiteSpace(code[i]) && (code[i] != '(')))
                return new ParseResult();
            var b = Parser.Parse(state, ref i, 1, true);
            if (b is EmptyStatement)
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Can't throw result of EmptyStatement " + Tools.PositionToTextcord(code, i - 1))));
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ThrowStatement()
                {
                    body = b
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            throw new JSException(body.Invoke(context));
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            Parser.Optimize(ref body, 2, varibles);
            return false;
        }

        public override string ToString()
        {
            return "throw" + (body != null ? " " + body : "");
        }
    }
}