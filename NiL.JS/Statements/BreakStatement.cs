using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    class BreakStatement : Statement
    {
        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            if (!Parser.Validate(code, "break", ref i))
                return new ParseResult();
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
