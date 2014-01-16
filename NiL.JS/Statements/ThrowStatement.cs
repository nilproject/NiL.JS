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
            if (!Parser.Validate(code, "throw", ref i))
                return new ParseResult();
            i++;
            var b = Parser.Parse(state, ref i, 1, true);
            if (b is EmptyStatement)
                throw new ArgumentException("Can't throw result of EmptyStatement");
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

        public override JSObject Invoke(Context context, JSObject args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref body, 2, varibles);
            return false;
        }
    }
}