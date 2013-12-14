using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ThrowStatement : Statement
    {
        private Statement body;

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "throw", ref i))
                return new ParseResult();
            i++;
            var b = Parser.Parse(state, ref i, 1);
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

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
