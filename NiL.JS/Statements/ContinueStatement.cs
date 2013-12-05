using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    class ContinueStatement : Statement
    {
        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            if (!Parser.Validate(code, "continue", ref i))
                return new ParseResult();
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ContinueStatement()
            };
        }

        public override JSObject Invoke(Context context)
        {
            context.abort = AbortType.Continue;
            context.abortInfo = null;
            return JSObject.undefined;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }
    }
}
