using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    class BreakStatement : Statement
    {
        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "break", ref i))
                return new ParseResult();
            if (state.AllowBreak <= 0)
                throw new ArgumentException();
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new BreakStatement()
            };
        }

        public override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Break;
            context.abortInfo = null;
            return JSObject.undefined;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
